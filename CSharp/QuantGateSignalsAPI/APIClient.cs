using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using QuantGate.API.Signals.ProtoStomp;
using QuantGate.API.Signals.Subscriptions;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace QuantGate.API.Signals
{
    /// <summary>
    /// Ultra simple protobuf STOMP client with command buffering support
    /// </summary>
    public class APIClient : IDisposable
    {
        /// <summary>
        /// Module-level identifier.
        /// </summary>
        private const string _moduleID = "PSCl";

        #region Events

        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };
        public event EventHandler<ErrorEventArgs> Error = delegate { };

        #endregion

        #region Subscription Mappings

        /// <summary>
        /// Dictionary of message consumers to handle each message type.
        /// </summary>
        private readonly Dictionary<ResponseFrame.ResponseOneofCase, Action<ResponseFrame>> _messageConsumers;
        /// <summary>
        /// Holds a list of all current subscriptions.
        /// </summary>
        private readonly Dictionary<ulong, ProtoStompSubscription> _subscriptionReferences =
            new Dictionary<ulong, ProtoStompSubscription>();
        /// <summary>
        /// Holds a list of all requests requiring a receipt.
        /// </summary>
        private readonly Dictionary<ulong, IReceiptable> _receiptReferences =
            new Dictionary<ulong, IReceiptable>();

        #endregion

        #region Connection Variables

        /// <summary>
        /// Is the client disconnecting?
        /// </summary>
        private bool _isDisconnecting = false;
        /// <summary>
        /// Is this object disposed yet?
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// The minimum number of ticks to wait to reconnect.
        /// </summary>
        private const long _minReconnect = 5 * TimeSpan.TicksPerSecond;
        /// <summary>
        /// The number of ticks to wait after a connection attempt before killing the attempt.
        /// </summary>
        private const long _connectKill = 20 * TimeSpan.TicksPerSecond;
        /// <summary>
        /// The maximum reconnection attempt count to use to adjust reconnection attempts.
        /// </summary>
        private const long _maxReconnect = 10;
        /// <summary>
        /// The number of times a reconnection has been attempted.
        /// </summary>
        private long _reconnectCount = 0;
        /// <summary>
        /// The next time to attempt a reconnection
        /// </summary>
        private long _reconnectTicks = 0;
        /// <summary>
        /// The time to kill a reconnection attempt.
        /// </summary>
        private long _killTicks = 0;

        /// <summary>
        /// The maximum time to wait before receiveing a message.
        /// </summary>
        private const long _maxHeartBeatWait = 1 * TimeSpan.TicksPerMinute;
        /// <summary>
        /// Time to wait after not receiving a message before sending a heartbeat request.
        /// </summary>
        private const long _heartBeatCheckTicks = 10 * TimeSpan.TicksPerSecond;
        /// <summary>
        /// The last time that a message was received.
        /// </summary>
        private long _lastMessageTicks = 0;

        #endregion

        #region Private and Internal Variables

        /// <summary>
        /// The dispatcher to use for threading.
        /// </summary>
        internal SynchronizationContext Sync { get; }

        /// <summary>
        /// Blocking message queue used in the main thread to process new actions within the thread.
        /// </summary>
        private readonly BlockingCollection<Action> _actions = new BlockingCollection<Action>();

        /// <summary>
        /// Transport layer interface instance.
        /// </summary>
        private readonly WebSocket _transport;

        /// <summary>
        /// Used to generate ids in messages, etc.
        /// </summary>
        internal IDGenerator IDGenerator { get; } = new IDGenerator();

        /// <summary>
        /// Stream ID associated with the stream the client is connected to.
        /// </summary>
        private string _streamID;

        /// <summary>
        /// The timer reference to use (if specified).
        /// </summary>
        private Timer _timer;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="StompClient" /> class.
        /// </summary>
        /// <param name="host">The web address to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="stream">The base datastream to connect to (default = realtime).</param>
        /// <param name="sync">The synchronization context to return values on (default = SychronizationContext.Current).</param>
        public APIClient(string host, int port = int.MinValue, DataStream stream = DataStream.Realtime, SynchronizationContext sync = null)
        {
            if (port == int.MinValue)
            {
                // If no port was specified, figure out the appropriate port.
                string[] fields = host.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (fields.Length > 1 && int.TryParse(fields[1], out port))
                    host = fields[0];
                else if (host.StartsWith("wss"))
                    port = 443;
                else
                    port = 80;
            }

            // Get a reference to the dispatcher to use.
            Sync = (sync ?? SynchronizationContext.Current) ?? new SynchronizationContext();

            Host = host;
            Port = port;

            // Set the stream (and get the proper stream ID).
            Stream = stream;
            switch (Stream)
            {
                case DataStream.Delayed: _streamID = ParsedDestination.DelayStreamID; break;
                case DataStream.Demo: _streamID = ParsedDestination.DemoStreamID; break;
                default: _streamID = ParsedDestination.RealtimeStreamID; break;
            }

            // Start the main (long running) thread queue.
            Task.Factory.StartNew(HandleActions, TaskCreationOptions.LongRunning);

            // Create the new websocket.
            _transport = new WebSocket(Host + ':' + Port + "/");

            // Set up the event handling.
            _transport.OnOpen += OnOpen;
            _transport.OnClose += OnClose;
            _transport.OnMessage += HandleMessage;
            _transport.OnError += OnError;

            _messageConsumers = new Dictionary<ResponseFrame.ResponseOneofCase, Action<ResponseFrame>>
            {
                [ResponseFrame.ResponseOneofCase.SingleMessage] = HandleMessageFrame,
                [ResponseFrame.ResponseOneofCase.BatchMessages] = HandleMessagesFrame,
                [ResponseFrame.ResponseOneofCase.Receipt] = HandleReceiptFrame,
                [ResponseFrame.ResponseOneofCase.SubscriptionError] = HandleSubscriptionError,
                [ResponseFrame.ResponseOneofCase.ServerError] = HandleErrorFrame,
                [ResponseFrame.ResponseOneofCase.Connected] = OnStompConnected,
                [ResponseFrame.ResponseOneofCase.Heartbeat] = HandleHeartbeatFrame,
            };

            _timer = new Timer(HandleTimer, null, 5000, 5000);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The host address of the server to connect to.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The port of the server to connect to.
        /// </summary>
        public int Port { get; }

        public string Username { get; private set; }
        public string Password { get; private set; }

        public DataStream Stream { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref = "APIClient" /> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; private set; }

        #endregion

        #region Thread Queue Handling

        private void Enqueue(Action action) { _actions.Add(action); }

        private void HandleActions()
        {
            foreach (Action action in _actions.GetConsumingEnumerable())
                action();
        }

        #endregion

        #region Transport Event Handling

        private void OnOpen(object source, EventArgs args)
        {
            Enqueue(() =>
            {
                ConnectRequest connect;

                try
                {
                    connect = new ConnectRequest { AcceptVersion = "1.0" };

                    if (Username is object)
                        connect.Login = Username;
                    if (Password is object)
                        connect.Passcode = Password;

                    Send(new RequestFrame { Connect = connect });
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":OO - " + ex.Message);
                }
            });
        }

        private void OnClose(object source, EventArgs args)
        {
            Enqueue(() =>
            {
                try
                {
                    if (_isDisconnecting || _isDisposed)
                    {
                        // If not reconnecting, clear any open subscriptions.
                        ClearSubscriptions();
                        // Not reconnecting.
                        _reconnectTicks = 0;
                    }
                    else
                    {
                        // Add to the reconnect count.
                        _reconnectCount += 1;
                        if (_reconnectCount > _maxReconnect)
                            _reconnectCount = _maxReconnect;

                        // Update the reconnect ticks value.
                        _reconnectTicks = DateTime.UtcNow.Ticks + _minReconnect * _reconnectCount;
                    }

                    // Set the status values.
                    IsConnected = false;
                    Sync.Post(new SendOrPostCallback((o) => { Disconnected(this, EventArgs.Empty); }), null);

                    // If disposed, stop the thread.
                    if (_isDisposed)
                        _actions.CompleteAdding();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":OCl - " + ex.Message);
                }
            });
        }

        private void OnError(object o, WebSocketSharp.ErrorEventArgs e)
        {
            Enqueue(() =>
            {
                try
                {
                    string message;

                    message = e.Message + (e.Exception?.Message ?? string.Empty);

                    // Handle the error message.
                    Trace.TraceError(_moduleID + ":HE - Stomp transport error: " + message);

                    // Make sure it closes properly.                        
                    if ((_transport is object) && (_transport.ReadyState == WebSocketState.Closed))
                        Close();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":OCl - " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Dispatches the given message to a registered message consumer.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void HandleMessage(object source, MessageEventArgs args)
        {
            Enqueue(() =>
            {
                ResponseFrame frame;

                try
                {
                    // Mark as the last time a message was received.
                    _lastMessageTicks = DateTime.UtcNow.Ticks;

                    // Parse the next message frame.
                    frame = ResponseFrame.Parser.ParseFrom(args.RawData);

                    // If parsed properly and the consumer can be found, call the consumer for the message.
                    if ((frame is object) && _messageConsumers.TryGetValue(frame.ResponseCase, out Action<ResponseFrame> consumer))
                        consumer(frame);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":HMsg - " + ex.Message);
                }
            });
        }

        #endregion

        #region Response Frame Handling

        /// <summary>
        /// Called when [connected] received.
        /// </summary>
        /// <param name="obj">The obj.</param>
        private void OnStompConnected(ResponseFrame frame)
        {
            try
            {
                // If connected, clear the reconnect properties.
                _reconnectCount = 0;
                _reconnectTicks = 0;
                _killTicks = 0;
                
                IsConnected = true;

                IDGenerator.Reset();
                ResubscribeAll();

                Sync.Post(new SendOrPostCallback((o) => { Connected(this, EventArgs.Empty); }), null);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":OSCn - " + ex.Message);
            }
        }

        private void HandleMessageFrame(ResponseFrame frame)
        {
            try
            {
                // Handle the single message.
                HandleMessage(frame.SingleMessage);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HMF - " + ex.Message);
            }
        }

        private void HandleMessagesFrame(ResponseFrame frame)
        {
            try
            {
                // Go through all the messages and handle individually.
                foreach (MessageResponse message in frame.BatchMessages.Message)
                    HandleMessage(message);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HMsF - " + ex.Message);
            }
        }

        private void HandleMessage(MessageResponse message)
        {
            ProtoStompSubscription subscription;

            try
            {
                // If the subscription id exists, try to get the subscription.                
                _subscriptionReferences.TryGetValue(message.SubscriptionId, out subscription);

                if (subscription is object)
                {
                    // If the subscription was found, handle the next message.
                    (subscription as IObserver<ByteString>).OnNext(message.Body);
                }
                else if (!_isDisconnecting && IsConnected)
                {
                    // If not disconnecting, log an error.
                    Trace.TraceInformation(_moduleID + ":HM - Subscription not found for id: " +
                                           message.SubscriptionId.ToString());
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HM - " + ex.Message);
            }
        }

        private void HandleReceiptFrame(ResponseFrame frame)
        {
            try
            {
                // Try to get the receiptable and remove if found.
                if (_receiptReferences.TryGetValue(frame.Receipt.ReceiptId, out IReceiptable receiptable))
                    _receiptReferences.Remove(frame.Receipt.ReceiptId);

                if (receiptable is object)
                {
                    receiptable.OnReceipt();
                }
                else if (!_isDisconnecting && IsConnected)
                {
                    Trace.TraceError(_moduleID + ":HRF - Receiptable not found for id" + frame.Receipt.ReceiptId.ToString());
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HRF - " + ex.Message);
            }
        }

        private void HandleErrorFrame(ResponseFrame frame)
        {
            Sync.Post(new SendOrPostCallback((o) => { Error(this, new ErrorEventArgs(frame.ServerError.Message)); }), null);
        }

        private void HandleSubscriptionError(ResponseFrame frame)
        {
            ProtoStompSubscription subscription;

            try
            {
                // If the subscription id exists, try to get the subscription.
                _subscriptionReferences.TryGetValue(
                    frame.SubscriptionError.SubscriptionId, out subscription);

                if (subscription is object)
                {
                    // If the subscription was found, handle the error. 
                    (subscription as IObserver<ByteString>).
                        OnError(new Exception(frame.SubscriptionError.Message));
                }
                else if (!_isDisconnecting && IsConnected)
                {
                    // If not disconnecting, log an error.
                    Trace.TraceInformation(_moduleID + ":HSE - Subscription not found for id: " +
                                           frame.SubscriptionError.SubscriptionId.ToString());
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HSE - " + ex.Message);
            }
        }

        /// <summary>
        /// Handles the next heartbeat frame.
        /// </summary>
        /// <param name="frame">The heartbeat frame to handle.</param>
        /// <remarks>Nothing to handle, since the last message time is already set.</remarks>
        private void HandleHeartbeatFrame(ResponseFrame frame) { }

        #endregion

        #region Timer Handling

        private void HandleTimer(object state)
        {
            Enqueue(() =>
            {
                long utcTicks;

                try
                {
                    // Get the current time.
                    utcTicks = DateTime.UtcNow.Ticks;

                    if (!IsConnected && !_isDisconnecting)
                    {
                        // If not connected, check if we need to reconnect.
                        if (utcTicks > _reconnectTicks && _reconnectTicks != 0)
                        {
                            Connect(Password);
                            _reconnectTicks = 0;
                            _killTicks = utcTicks + _connectKill;
                        }
                        else if (utcTicks > _killTicks && _killTicks != 0)
                        {
                            Disconnect(false);
                            _killTicks = 0;
                        }
                    }
                    else if (IsConnected && !_isDisconnecting)
                    {
                        // If connected and not disconnecting.
                        if (utcTicks > _lastMessageTicks + _maxHeartBeatWait)
                        {
                            // If it's been too long before receiving a message, disconnect (to reconnect).
                            Disconnect(false);
                        }
                        else if (utcTicks > _lastMessageTicks + _heartBeatCheckTicks)
                        {
                            // If past the last heartbeat checks, request a heartbeat.
                            Send(new RequestFrame { Heartbeat = new Heartbeat() });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":Tmr - " + ex.Message);
                    OnClose(this, EventArgs.Empty);
                }
            });
        }

        #endregion

        #region Connection Handling

        /// <summary>
        /// Connects to the server on the specified address.
        /// </summary>
        /// <param name="jwtToken">Jwt Token to connect with.</param>
        public void Connect(string jwtToken)
        {
            Enqueue(() =>
            {
                byte[] bytes;
                string username;

                try
                {
                    // Get the username from the token.
                    bytes = Convert.FromBase64String(jwtToken.Split(new char[] { '.' })[1]);
                    username = System.Text.Encoding.UTF8.GetString(bytes);
                    username = username.Split(new string[] { "sub\":\"" }, StringSplitOptions.None)[1].Split(new char[] { '"' })[0];

                    // Set the username and password.
                    Password = jwtToken;
                    Username = username;

                    _isDisconnecting = false;
                    _reconnectTicks = 0;
                    _killTicks = DateTime.UtcNow.Ticks + _connectKill;

                    // Connect to the websocket.
                    _transport.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":Cn - " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            Enqueue(() => { Disconnect(true); });
        }

        private void Disconnect(bool disconnecting)
        {
            try
            {
                if (disconnecting)
                {
                    _isDisconnecting = true;
                    ClearSubscriptions();
                }

                Send(new RequestFrame { Disconnect = new DisconnectRequest() });

                Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":DCn - " + ex.Message);
            }
        }

        /// <summary>
        /// Used to close the transport object.
        /// </summary>
        private void Close()
        {
            try
            {
                if (_transport is object)
                {
                    if (_transport.ReadyState != WebSocketState.Closed)
                        _transport.CloseAsync();
                    else
                        OnClose(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cls - " + ex.Message);
                OnClose(this, EventArgs.Empty);
            }
        }

        private void Send(RequestFrame frame)
        {
            try
            {
                _transport.SendAsync(frame.ToByteArray(), null);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Snd - " + ex.Message);
            }
        }

        #endregion

        #region Subscription Handling

        /// <summary>
        /// Subscribes to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="reference">A reference object to associate with this request.</param>
        internal void Subscribe(ProtoStompSubscription subscription)
        {
            Enqueue(() =>
            {
                try
                {
                    if (subscription is object)
                    {
                        _subscriptionReferences[subscription.SubscriptionID] = subscription;

                        if (subscription.ReceiptID != 0)
                            _receiptReferences.Add(subscription.ReceiptID, subscription);

                        if (IsConnected & !_isDisconnecting)
                        {
                            // If connected, send the subscription request - otherwise, waiting for connection.
                            Send(new RequestFrame { Subscribe = subscription.Request });

                            // Log the subscription action.
                            Trace.TraceInformation(_moduleID + ":Sub - Subscribe: " + subscription.Destination +
                                                    " [" + subscription.SubscriptionID.ToString() + "]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":Sub - " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Unsubscribes the specified destination.
        /// </summary>
        internal void Throttle(ProtoStompSubscription subscription, uint rate)
        {
            Enqueue(() =>
            {
                ThrottleRequest throttle = new ThrottleRequest();
                ProtoStompReceipt receipt = new ProtoStompReceipt(IDGenerator.NextID);

                try
                {
                    if (subscription is object)
                    {
                        // Create the unsubscribe message.
                        throttle.SubscriptionId = subscription.SubscriptionID;
                        throttle.ThrottleRate = rate;
                        throttle.ReceiptId = receipt.ReceiptID;

                        // Add to the receiptable requests.                        
                        _receiptReferences.Add(receipt.ReceiptID, receipt);

                        if (IsConnected & !_isDisconnecting)
                        {
                            // If connected, send the throttle request - if not sent, will be applied to the initial subscription.
                            Send(new RequestFrame { Throttle = throttle });

                            // Log the throttle action.
                            Trace.TraceInformation(_moduleID + ":Thr - Throttle: " + subscription.Destination +
                                                 " [" + subscription.SubscriptionID.ToString() + "]: " + rate.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":Thr - " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Unsubscribes the specified subscription.
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe from.</param>
        internal void Unsubscribe(ProtoStompSubscription subscription)
        {
            Enqueue(() =>
            {
                ProtoStompReceipt receipt = new ProtoStompReceipt(IDGenerator.NextID);
                UnsubscribeRequest unsubscribe = new UnsubscribeRequest();

                try
                {
                    if (subscription is object)
                    {
                        // Remove from the subscription references.                        
                        _subscriptionReferences.Remove(subscription.SubscriptionID);

                        // Create the unsubscribe message.
                        unsubscribe.SubscriptionId = subscription.SubscriptionID;
                        unsubscribe.ReceiptId = receipt.ReceiptID;

                        // Handle the receipt events on the subscription.
                        receipt.Invalidated += ((IReceiptable)subscription).OnInvalidate;
                        receipt.Receipted += ((IObserver<ByteString>)subscription).OnCompleted;

                        // Add to the receiptable requests.                        
                        _receiptReferences.Add(receipt.ReceiptID, receipt);

                        if (IsConnected & !_isDisconnecting)
                        {
                            // If connected, send the message.
                            Send(new RequestFrame { Unsubscribe = unsubscribe });

                            // Log the subscription action.
                            Trace.TraceInformation(_moduleID + ":USub - Unsubscribe: " +
                                                   subscription.Destination + " [" + subscription.SubscriptionID.ToString() + "]");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":USub - " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Sends a message to the specified address.
        /// </summary>
        /// <param name="toSend">The Stomp frame to send.</param>
        internal void Send(ProtoStompSend toSend)
        {
            Enqueue(() =>
            {
                try
                {
                    if (toSend.ReceiptID != 0)
                        _receiptReferences.Add(toSend.ReceiptID, toSend);

                    Send(new RequestFrame { Send = toSend.Request });
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":Snd - " + ex.Message);
                }
            });
        }

        private void ResubscribeAll()
        {
            List<ProtoStompSubscription> subscriptions;

            try
            {
                // Get the current subscriptions list and clear the old.
                subscriptions = _subscriptionReferences.Values.ToList();
                _subscriptionReferences.Clear();
                _receiptReferences.Clear();

                foreach (ProtoStompSubscription subscription in subscriptions)
                {
                    // Go through all the subscriptions, generate a new receipt ID if necessary.
                    if (subscription.ReceiptID != 0)
                        subscription.ReceiptID = IDGenerator.NextID;

                    // Generate a new subscription ID.
                    subscription.SubscriptionID = IDGenerator.NextID;

                    // Subscribe to the subscription.
                    subscription.Subscribe();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":ReSub - " + ex.Message);
            }
        }

        /// <summary>
        /// Clears subscriptions and receipt subscriptions.
        /// </summary>
        private void ClearSubscriptions()
        {
            List<ProtoStompSubscription> subscriptions = null;
            List<IReceiptable> receiptables = null;

            try
            {
                // Clear the subscription references.
                subscriptions = _subscriptionReferences.Values.ToList();
                _subscriptionReferences.Clear();

                // Handle OnCompleted events for each subscription.
                foreach (IObserver<ByteString> subscription in subscriptions)
                    subscription.OnCompleted();

                // Clear the receipt references.
                receiptables = _receiptReferences.Values.ToList();
                _receiptReferences.Clear();

                // Handle the OnInvalidate events for each subscription.
                foreach (IReceiptable receiptable in receiptables)
                    receiptable.OnInvalidate();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CSs - " + ex.Message);
            }
            finally
            {
                subscriptions?.Clear();
                receiptables?.Clear();
            }
        }

        #endregion

        #region Subscriptions

        /// <summary>
        /// Subscribes to a Perception gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Perception data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Perception data object that will receive the updates.</returns>
        public Perception SubscribePerception(string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new PerceptionSubscription(this, _streamID, symbol, throttleRate: (uint)throttleRate)) as Perception;

        /// <summary>
        /// Subscribes to a Commitment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Commitment data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Commitment data object that will receive the updates.</returns>
        public Commitment SubscribeCommitment(string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new CommitmentSubscription(this, _streamID, symbol, throttleRate: (uint)throttleRate)) as Commitment;

        /// <summary>
        /// Subscribes to a Book Pressure gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Book Pressure data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Book Pressure data object that will receive the updates.</returns>
        public BookPressure SubscribeBookPressure(string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new BookPressureSubscription(this, _streamID, symbol, throttleRate: (uint)throttleRate)) as BookPressure;

        /// <summary>
        /// Subscribes to a Headroom gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Headroom data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Headroom data object that will receive the updates.</returns>
        public Headroom SubscribeHeadroom(string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new HeadroomSubscription(this, _streamID, symbol, throttleRate: (uint)throttleRate)) as Headroom;

        /// <summary>
        /// Subscribes to a Sentiment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Sentiment data for.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 50t.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Sentiment data object that will receive the updates.</returns>
        public Sentiment SubscribeSentiment(string symbol, string compression = "50t", int throttleRate = 0) =>
            SubscribeAndReturn(new SentimentSubscription(this, _streamID, symbol, compression,
                                                         throttleRate: (uint)throttleRate)) as Sentiment;

        /// <summary>
        /// Subscribes to a Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Equilibrium data for.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 50t.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Equilibrium data object that will receive the updates.</returns>
        public Equilibrium SubscribeEquilibrium(string symbol, string compression = "300s", int throttleRate = 0) =>
            SubscribeAndReturn(new EquilibriumSubscription(this, _streamID, symbol, compression,
                                                           throttleRate: (uint)throttleRate)) as Equilibrium;

        /// <summary>
        /// Subscribes to a Multiframe Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Multiframe Equilibrium data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Multiframe Equilibrium data object that will receive the updates.</returns>
        public MultiframeEquilibrium SubscribeMultiframeEquilibrium(string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new MultiframeSubscription(this, _streamID, symbol,
                                                          throttleRate: (uint)throttleRate)) as MultiframeEquilibrium;

        /// <summary>
        /// Subscribes to a Trigger gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Trigger data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Trigger data object that will receive the updates.</returns>
        public Trigger SubscribeTrigger(string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new TriggerSubscription(this, _streamID, symbol, throttleRate: (uint)throttleRate)) as Trigger;

        /// <summary>
        /// Subscribes to a Strategy update data stream for a specific strategy and symbol.
        /// </summary>
        /// <param name="strategyID">Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0,  Crb.8.4.</param>
        /// <param name="symbol">Symbol to get the Strategy update data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Strategy data object that will receive the updates.</returns>
        public StrategyValues SubscribeStrategy(string strategyID, string symbol, int throttleRate = 0) =>
            SubscribeAndReturn(new StrategySubscription(this, strategyID, _streamID, symbol,
                                                        throttleRate: (uint)throttleRate)) as StrategyValues;

        /// <summary>
        /// Used to retrieve the specification details of an instrument according to its symbol,
        /// as listed by the QuantGate servers..
        /// </summary>        
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <returns>The Instrument data object that will receive the updates.</returns>
        /// <remarks>The client should unsubscribe as soon as the data is received.</remarks>
        public Instrument SubscribeInstrument(string symbol) =>
            SubscribeAndReturn(new InstrumentSubscription(this, _streamID, symbol)) as Instrument;

        /// <summary>
        /// Subscribes to a stream of Top Symbols according to broker and instrument type.
        /// </summary>
        /// <param name="broker">The broker to get the Top Symbols for. Must match a valid broker type string.</param>
        /// <param name="instrumentType">The type of instrument to include in the results.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Top Symbols data object that will receive the updates.</returns>
        public TopSymbols SubscribeTopSymbols(string broker, InstrumentType instrumentType =
                                              InstrumentType.NoInstrument, int throttleRate = 0) =>
            SubscribeAndReturn(new TopSymbolsSubscription(this, null, broker, instrumentType,
                                                          throttleRate: (uint)throttleRate)) as TopSymbols;

        /// <summary>
        /// Subscribes to a stream and returns the values from the stream.
        /// </summary>
        /// <param name="subscription">The subscription to subscribe to and return.</param>
        /// <returns>The values data object that will receive the subscription updates.</returns>
        private ValueBase SubscribeAndReturn(ISubscription subscription)
        {
            subscription.Subscribe();
            return subscription.Values;
        }

        /// <summary>
        /// Subscribes to a search stream and returns an object from which to request and receive search results.
        /// </summary>
        /// <returns>A Symbol Search object that can be used to search for and receive search results.</returns>
        public SymbolSearch SubscribeSearch()
        {
            SearchSubscription subscription = new SearchSubscription(this, _streamID);
            subscription.Subscribe();
            return new SymbolSearch(subscription);
        }

        /// <summary>
        /// Unsubscribes the from the stream for the given values.
        /// </summary>
        /// <param name="values">The values to unsubscribe the stream from.</param>
        public void Unsubscribe(ValueBase values) => values.Unsubscribe();

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // Dispose of and clear the timer reference.
                _timer?.Dispose();
                _timer = null;

                // If still connected, disconnect, otherwise stop the thread.
                if (IsConnected)
                    Disconnect();
                else
                    _actions.CompleteAdding();

                _isDisposed = true;
            }
        }

        ~APIClient()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
