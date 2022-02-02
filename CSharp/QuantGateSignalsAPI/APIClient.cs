using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using QuantGate.API.Signals.ProtoStomp;
using QuantGate.API.Signals.Subscriptions;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using events = QuantGate.API.Events;
using System.Net;
using System.IO;
using Epcylon.Net.APIs.Account;

namespace QuantGate.API.Signals
{
    /// <summary>
    /// QuantGate signals API client, with automatic reconnection and subscription handling.
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
        public event EventHandler<events.ErrorEventArgs> Error = delegate { };

        public event EventHandler<PerceptionEventArgs> PerceptionUpdated = delegate { };
        public event EventHandler<CommitmentEventArgs> CommitmentUpdated = delegate { };
        public event EventHandler<EquilibriumEventArgs> EquilibriumUpdated = delegate { };
        public event EventHandler<SentimentEventArgs> SentimentUpdated = delegate { };
        public event EventHandler<BookPressureEventArgs> BookPressureUpdated = delegate { };
        public event EventHandler<HeadroomEventArgs> HeadroomUpdated = delegate { };
        public event EventHandler<MultiframeEquilibriumEventArgs> MultiframeEquilibriumUpdated = delegate { };
        public event EventHandler<TriggerEventArgs> TriggerUpdated = delegate { };
        public event EventHandler<SearchResultsEventArgs> SymbolSearchUpdated = delegate { };
        public event EventHandler<TopSymbolsEventArgs> TopSymbolsUpdated = delegate { };
        public event EventHandler<StrategyEventArgs> StrategyUpdated = delegate { };
        public event EventHandler<InstrumentEventArgs> InstrumentUpdated = delegate { };
        public event EventHandler<FuturesListEventArgs> FuturesListUpdated = delegate { };

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
        /// Holds a list of all current subscriptions by destination.
        /// </summary>
        private readonly Dictionary<string, ProtoStompSubscription> _subscriptionsByDestination = 
            new Dictionary<string, ProtoStompSubscription>();

        /// <summary>
        /// Holds a list of all requests requiring a receipt.
        /// </summary>
        private readonly Dictionary<ulong, IReceiptable> _receiptReferences = new Dictionary<ulong, IReceiptable>();

        /// <summary>
        /// Search subscription.
        /// </summary>
        private SearchSubscription _search = null;

        #endregion

        #region Connection Variables

        /// <summary>
        /// The host address of the server to connect to.
        /// </summary>
        private readonly string _host;
        /// <summary>
        /// The port of the server to connect to.
        /// </summary>
        private readonly int _port;

        /// <summary>
        /// The password for the current connection (JWT Token).
        /// </summary>        
        private ConnectionToken _token;
        

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
        /// The next time to attempt a reconnection.
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

        /// <summary>
        /// Epoch time for calculating UNIX time.
        /// </summary>
        private readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

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
        private readonly string _streamID;

        /// <summary>
        /// The timer reference to use (if specified).
        /// </summary>
        private readonly Timer _timer;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="StompClient" /> class.
        /// </summary>
        /// <param name="environment">The server environment to connect to.</param>
        /// <param name="stream">The base datastream to connect to (default = realtime).</param>
        /// <param name="sync">
        /// The synchronization context to return values on (default = SychronizationContext.Current).
        /// </param>
        public APIClient(ConnectionToken token, DataStream stream = DataStream.Realtime, 
                         SynchronizationContext sync = null)
        {
            // Get a reference to the dispatcher to use.
            Sync = (sync ?? SynchronizationContext.Current) ?? new SynchronizationContext();

            // Set the environment
            _token = token;
            switch (_token.Environment)
            {
                case Environments.Local:
                    _host = @"ws://localhost";
                    _port = 2432;
                    break;
                case Environments.Development:
                    _host = @"wss://dev.stealthtrader.com";
                    _port = 443;
                    break;
                case Environments.Staging:
                    _host = @"wss://test.stealthtrader.com";
                    _port = 443;
                    break;
                case Environments.Production:
                    _host = @"wss://feed.stealthtrader.com";
                    _port = 443;
                    break;
            }                        

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
            _transport = new WebSocket($"{_host}:{_port}/");

            // Set up the event handling.
            _transport.OnOpen += OnOpen;
            _transport.OnClose += OnClose;
            _transport.OnMessage += HandleMessage;
            _transport.OnError += OnError;

            // Set up the message consumers (dictionary of handlers for each response message type).
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

            // Create a new timer to handle disconnections/reconnections, etc.
            _timer = new Timer(HandleTimer, new object(), 5000, 5000);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The server environment to connect to.
        /// </summary>
        public Environments Environment => _token.Environment;

        /// <summary>
        /// The username (email) of the connected user.
        /// </summary>
        public string Username { get; private set; }        

        /// <summary>
        /// Returns the type of stream that this client is connected to (realtime/delay/demo).
        /// </summary>
        public DataStream Stream { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref = "APIClient"/> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; private set; }

        #endregion

        #region Thread Queue Handling

        /// <summary>
        /// Adds a new action to the list of actions to run in the main thread.
        /// </summary>
        /// <param name="action">The action to run within the thread.</param>
        internal void Enqueue(Action action) { _actions.Add(action); }

        /// <summary>
        /// This is the main thread loop. Handles actions from a blocking collection until cancelled.
        /// </summary>
        private void HandleActions()
        {
            // Go through each action in the blocking collection while not cancelled.
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

                    if (_token is object)
                    {
                        connect.Login = _token.ClientID;
                        connect.Passcode = _token.Token;
                    }

                    Send(new RequestFrame { Connect = connect });
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"{_moduleID}:OO - {ex.Message}");
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
                    Sync.Post(new SendOrPostCallback(o => { Disconnected(this, EventArgs.Empty); }), null);

                    // If disposed, stop the thread.
                    if (_isDisposed)
                        _actions.CompleteAdding();
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"{_moduleID}:OCl - {ex.Message}");
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
                    Trace.TraceError($"{_moduleID}:OErr - {ex.Message}");
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
                    if ((frame is object) && _messageConsumers.TryGetValue(frame.ResponseCase, out var consumer))
                        consumer(frame);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"{_moduleID}:HMsg - {ex.Message}");
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

                Sync.Post(new SendOrPostCallback(o => { Connected(this, EventArgs.Empty); }), null);
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
            try
            {
                // If the subscription id exists, try to get the subscription.                
                _subscriptionReferences.TryGetValue(message.SubscriptionId, out var subscription);

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
                if (_receiptReferences.TryGetValue(frame.Receipt.ReceiptId, out var receiptable))
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
            Sync.Post(new SendOrPostCallback(o => { Error(this, new events.ErrorEventArgs(frame.ServerError.Message)); }), null);
        }

        private void HandleSubscriptionError(ResponseFrame frame)
        {
            try
            {
                // If the subscription id exists, try to get the subscription.
                _subscriptionReferences.TryGetValue(
                    frame.SubscriptionError.SubscriptionId, out var subscription);

                if (subscription is object)
                {
                    // If the subscription was found, handle the error. 
                    (subscription as IObserver<ByteString>).
                        OnError(new Exception(frame.SubscriptionError.Message,
                                              new Exception(frame.SubscriptionError.Details)));
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
        private void HandleHeartbeatFrame(ResponseFrame frame)
        {
            // Mark as the last time a message was received.
            _lastMessageTicks = DateTime.UtcNow.Ticks;
        }

        #endregion        

        #region Connection Handling        

        /// <summary>
        /// Connects to the server on the specified address.
        /// </summary>
        public void Connect()
        {
            Enqueue(() =>
            {
                try
                {                    
                    // Set the username and password.
                    _isDisconnecting = false;
                    _reconnectTicks = 0;
                    _killTicks = DateTime.UtcNow.Ticks + _connectKill;

                    // Connect to the websocket.
                    _transport.Connect();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":Cn2 - " + ex.Message);
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

        /// <summary>
        /// Disconnect from the websocket server.
        /// </summary>
        /// <param name="full">True if this is a full disconnect.</param>
        private void Disconnect(bool full)
        {
            try
            {
                if (full)
                {
                    // If doing a full disconnect, set to disconnecting.
                    _isDisconnecting = true;
                    // Clear the subscriptions.
                    ClearSubscriptions();
                }

                // Send disconnect frame and close the connection.
                Send(new RequestFrame { Disconnect = new DisconnectRequest() });
                Close();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":DCn - " + ex.Message);
            }
        }

        /// <summary>
        /// Used to close the APIClient connection.
        /// </summary>
        private void Close()
        {
            try
            {
                if (_transport is object)
                {
                    // If there is a transport, close if not closed, otherwise send event.
                    if (_transport.ReadyState != WebSocketState.Closed)
                        _transport.Close();
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

        /// <summary>
        /// Sends the given request frame to the WebSocket endpoint.
        /// </summary>
        /// <param name="frame">The frame to send.</param>
        private void Send(RequestFrame frame)
        {
            try
            {
                _transport.Send(frame.ToByteArray());
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
            try
            {
                if (subscription is object)
                {
                    _subscriptionReferences[subscription.SubscriptionID] = subscription;
                    _subscriptionsByDestination[subscription.Destination] = subscription;

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
        }

        /// <summary>
        /// Throttles the specified subscription.
        /// </summary>
        internal void Throttle(ProtoStompSubscription subscription, uint rate)
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
        }

        /// <summary>
        /// Unsubscribes the specified subscription.
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe from.</param>
        internal void Unsubscribe(ProtoStompSubscription subscription)
        {
            ProtoStompReceipt receipt = new ProtoStompReceipt(IDGenerator.NextID);
            UnsubscribeRequest unsubscribe = new UnsubscribeRequest();

            try
            {
                if (subscription is object)
                {
                    // Remove from the subscription references.                        
                    _subscriptionReferences.Remove(subscription.SubscriptionID);
                    _subscriptionsByDestination.Remove(subscription.Destination);

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
                        Trace.TraceInformation(_moduleID + ":USub - Unsubscribe: " + subscription.Destination +
                                               " [" + subscription.SubscriptionID.ToString() + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":USub - " + ex.Message);
            }
        }

        /// <summary>
        /// Sends a message to the specified address.
        /// </summary>
        /// <param name="toSend">The Stomp frame to send.</param>
        internal void Send(ProtoStompSend toSend)
        {            
            try
            {
                // If there is a receipt requested, add to receipt references.
                if (toSend.ReceiptID != 0)
                    _receiptReferences.Add(toSend.ReceiptID, toSend);

                // Send the Send request.
                Send(new RequestFrame { Send = toSend.Request });
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Snd - " + ex.Message);
            }            
        }

        /// <summary>
        /// Resubscribes all current subscriptions to the back-end
        /// (after disconnect/initial connection - i.e. when not present in current connection).
        /// </summary>
        private void ResubscribeAll()
        {
            List<ProtoStompSubscription> subscriptions;

            try
            {
                // Get the current subscriptions list and clear the old.
                subscriptions = _subscriptionReferences.Values.ToList();
                _subscriptionReferences.Clear();
                _subscriptionsByDestination.Clear();
                _receiptReferences.Clear();

                foreach (ProtoStompSubscription subscription in subscriptions)
                {
                    // Go through all the subscriptions, generate a new receipt ID if necessary.
                    if (subscription.ReceiptID != 0)
                        subscription.ReceiptID = IDGenerator.NextID;

                    // Generate a new subscription ID.
                    subscription.SubscriptionID = IDGenerator.NextID;

                    // Subscribe to the subscription.
                    Subscribe(subscription);
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
            List<ProtoStompSubscription> subscriptions;
            List<IReceiptable> receiptables;

            try
            {
                // Clear the subscription references.
                subscriptions = _subscriptionReferences.Values.ToList();
                _subscriptionReferences.Clear();
                _subscriptionsByDestination.Clear();
                _search = null;

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
        }

        #endregion

        #region Subscription Creation Methods

        /// <summary>
        /// Subscribes to a Perception gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Perception data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribePerception(string symbol, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new PerceptionSubscription(this, PerceptionUpdated, _streamID, symbol, 
                                                           false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Commitment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Commitment data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeCommitment(string symbol, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new CommitmentSubscription(this, CommitmentUpdated, _streamID, symbol, 
                                                           false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Book Pressure gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Book Pressure data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeBookPressure(string symbol, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new BookPressureSubscription(this, BookPressureUpdated, _streamID, 
                                                             symbol, false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Headroom gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Headroom data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeHeadroom(string symbol, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new HeadroomSubscription(this, HeadroomUpdated, _streamID, symbol, 
                                                         false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Sentiment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Sentiment data for.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 50t.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeSentiment(string symbol, string compression = "50t",
                                       int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new SentimentSubscription(this, SentimentUpdated, _streamID, symbol,
                                                          CleanCompression(compression), false, 
                                                          (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Equilibrium data for.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 300s.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeEquilibrium(string symbol, string compression = "300s", 
                                         int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new EquilibriumSubscription(this, EquilibriumUpdated, _streamID, symbol, 
                                                            CleanCompression(compression), false,
                                                            (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Multiframe Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Multiframe Equilibrium data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeMultiframeEquilibrium(string symbol, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new MultiframeSubscription(this, MultiframeEquilibriumUpdated, _streamID, 
                                                           symbol, false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Trigger gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Trigger data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeTrigger(string symbol, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new TriggerSubscription(this, TriggerUpdated, _streamID, symbol, 
                                                        false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Strategy update data stream for a specific strategy and symbol.
        /// </summary>
        /// <param name="strategyID">Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.</param>
        /// <param name="symbol">Symbol to get the Strategy update data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>        
        public void SubscribeStrategy(string strategyID, string symbol,
                                      int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new StrategySubscription(this, StrategyUpdated, strategyID, _streamID, 
                                                         symbol, false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a stream of Top Symbols according to broker and instrument type.
        /// </summary>
        /// <param name="broker">The broker to get the Top Symbols for. Must match a valid broker type string.</param>
        /// <param name="instrumentType">The type of instrument to include in the results.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Top Symbols data object that will receive the updates.</returns>
        public void SubscribeTopSymbols(string broker, InstrumentType instrumentType =
                                        InstrumentType.NoInstrument, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new TopSymbolsSubscription(this, TopSymbolsUpdated, broker, instrumentType,
                                                           false, (uint)throttleRate, reference));

        /// <summary>
        /// Used to retrieve the specification details of an instrument according to its symbol,
        /// as listed by the QuantGate servers.
        /// </summary>        
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        public void RequestInstrumentDetails(string symbol) =>
            EnqueueAndSubscribe(new InstrumentSubscription(this, InstrumentUpdated, _streamID, symbol));

        /// <summary>
        /// Used to retrieve a list of futures for the given underlying and currency, as listed by the QuantGate servers.
        /// </summary>
        /// <param name="underlying">The underlying of the futures to retrieve.</param>
        /// <param name="currency">The currency of the futures to retrieve.</param>
        public void RequestFutures(string underlying, string currency) =>
            EnqueueAndSubscribe(new FuturesListSubscription(this, FuturesListUpdated, _streamID, underlying, currency));

        /// <summary>
        /// Enqueues and subscribes to a stream.
        /// </summary>
        /// <param name="subscription">The subscription to subscribe to.</param>
        private void EnqueueAndSubscribe<M, V>(SubscriptionBase<M, V> subscription)
            where M : IMessage<M>
            where V : EventArgs
        {
            Enqueue(() =>
            {
                // If not yet subscribed, set the parent event handler and subscribe.
                if (!_subscriptionsByDestination.ContainsKey(subscription.Destination))
                    Subscribe(subscription);
            });
        }

        /// <summary>
        /// Requests symbols that match a specific term and (optionally) a specific broker. 
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <param name="broker">Broker to get the results for. If supplied, must match a valid broker type string.</param>
        public void SearchSymbols(string term, string broker)
        {
            Enqueue(() =>
            {
                if (_search is null)
                {
                    _search = new SearchSubscription(this, SymbolSearchUpdated, _streamID);
                    EnqueueAndSubscribe(_search);
                }

                _search.Search(term, broker);
            });
        }

        #endregion

        #region Unsubscription Methods

        /// <summary>
        /// Unsubscribes from a Perception gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Perception data for.</param>
        public void UnsubscribePerception(string symbol) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugePerception, symbol));

        /// <summary>
        /// Unsubscribes from a Commitment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Commitment data for.</param>
        public void UnsubscribeCommitment(string symbol) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeCommitment, symbol, "1m"));

        /// <summary>
        /// Unsubscribes from a Book Pressure gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Book Pressure data for.</param>
        public void UnsubscribeBookPressure(string symbol) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeBookPressure, symbol, "0q"));

        /// <summary>
        /// Unsubscribes from a Headroom gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Headroom data for.</param>
        public void UnsubscribeHeadroom(string symbol) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeHeadroom, symbol, "5m"));

        /// <summary>
        /// Unsubscribes from a Sentiment gauge data stream at the specified compression for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Sentiment data for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 50t.</param>
        public void UnsubscribeSentiment(string symbol, string compression = "50t") =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeSentiment, symbol, compression));

        /// <summary>
        /// Unsubscribes from an Equilibrium gauge data stream at the specified compression for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Equilibrium data for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 300s.</param>
        public void UnsubscribeEquilibrium(string symbol, string compression = "300s") =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeEquilibrium, symbol, compression));

        /// <summary>
        /// Unsubscribes from a Multiframe Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Multiframe Equilibrium data for.</param>
        public void UnsubscribeMultiframeEquilibrium(string symbol) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeMultiframeEquilibrium, symbol));

        /// <summary>
        /// Unsubscribes from a Trigger data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Triger data for.</param>
        public void UnsubscribeTrigger(string symbol) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeTrigger, symbol));

        /// <summary>
        /// Unsubscribes from Stategy data for the given strategy and symbol.
        /// </summary>
        /// <param name="strategyID">The identifier of the strategy to stop running.</param>
        /// <param name="symbol">The symbol to stop getting Strategy data for.</param>
        public void UnsubscribeStrategy(string strategyID, string symbol)
        {
            string destination = ParsedDestination.CreateStrategyDestination(
                strategyID, ParsedDestination.StreamIDForSymbol(_streamID, symbol), symbol).Destination;

            Unsubscribe(destination);
        }

        /// <summary>
        /// Unsubscribes from Top Symbols data for the given broker and instrument type.
        /// </summary>
        /// <param name="broker">The broker to stop getting the Top Symbols for.</param>
        /// <param name="instrumentType">The type of instrument to stop including from the results.</param>
        public void UnsubscribeTopSymbols(string broker, InstrumentType instrumentType = InstrumentType.NoInstrument)
        {
            string destination = ParsedDestination.CreateTopSymbolsDestination(
                broker, TopSymbolsSubscription.InstrumentTypeToString(instrumentType)).Destination;

            Unsubscribe(destination);
        }

        /// <summary>
        /// Used to unsubscribe from a stream of data with the destination supplied.
        /// </summary>
        /// <param name="destination">The destination of the stream to stop getting data for.</param>
        private void Unsubscribe(string destination)
        {
            Enqueue(() =>
            {
                // Make sure there is a leading slash.
                if (!destination.StartsWith("/"))
                    destination = '/' + destination;

                // If the destination exists, unsubscribe (otherwise, no need).
                if (_subscriptionsByDestination.TryGetValue(destination, out var subscription))
                    Unsubscribe(subscription);
            });
        }

        /// <summary>
        /// Unsubscribes from all active subscriptions.
        /// </summary>
        public void UnsubscribeAll()
        {
            Enqueue(() =>
            {
                // Get a list of all of the current subscriptions.
                List<ProtoStompSubscription> subscriptions = _subscriptionReferences.Values.ToList();

                // If no symbol supplied, unsubscribe from everything.
                foreach (ProtoStompSubscription subscription in subscriptions)
                    Unsubscribe(subscription);

                // This includes the search.
                _search = null;
                
            });
        }

        /// <summary>
        /// Unsubscribes from all subscriptions for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to unsubscribe from.</param>
        public void UnsubscribeAll(string symbol)
        {
            Enqueue(() =>
            {
                // Get a list of all of the current subscriptions.
                List<ProtoStompSubscription> subscriptions = _subscriptionReferences.Values.ToList();
               
                // If a symbol was supplied, need to check each subscription.
                foreach (ProtoStompSubscription subscription in subscriptions) 
                {
                    // If the subscription is tied to the symbol, unsubscribe.
                    if ((subscription is ISymbolSubscription symbolSubscription) && 
                        (symbolSubscription.Symbol == symbol))                        
                        Unsubscribe(subscription);
                }
            });
        }

        #endregion

        #region Subscription Throttling Methods

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Perception gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Perception gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottlePerception(string symbol, int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugePerception, symbol), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Commitment gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Commitment gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleCommitment(string symbol, int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeCommitment, symbol, "1m"), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Book Pressure gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Book Pressure gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleBookPressure(string symbol, int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeBookPressure, symbol, "0q"), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Headroom gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Headroom gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleHeadroom(string symbol, int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeHeadroom, symbol, "5m"), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Sentiment gauge updates for a 
        /// specific symbol at the specified compression.
        /// </summary>
        /// <param name="symbol">The symbol to change the Sentiment gauge throttle rate for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 50t.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleSentiment(string symbol, string compression = "50t", int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeSentiment, symbol, compression), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Equilibrium gauge updates for a 
        /// specific symbol at the specified compression.
        /// </summary>
        /// <param name="symbol">The symbol to change the Equilibrium gauge throttle rate for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 300s.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleEquilibrium(string symbol, string compression = "300s", int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeEquilibrium, symbol, compression), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Multiframe Equilibrium gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Multiframe Equilibrium gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleMultiframeEquilibrium(string symbol, int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeMultiframeEquilibrium, symbol), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Trigger updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Trigger throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleTrigger(string symbol, int throttleRate = 0) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeTrigger, symbol), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Strategy updates for the 
        /// given strategy and symbol.
        /// </summary>
        /// <param name="strategyID">The identifier of the strategy to throttle.</param>
        /// <param name="symbol">The symbol to change the Strategy throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleStrategy(string strategyID, string symbol, int throttleRate = 0)
        {
            string destination = ParsedDestination.CreateStrategyDestination(
                strategyID, ParsedDestination.StreamIDForSymbol(_streamID, symbol), symbol).Destination;

            Throttle(destination, throttleRate);
        }

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Top Symbols updates for the 
        /// given broker and instrument type.
        /// </summary>
        /// <param name="broker">The broker to throttle the Top Symbols for.</param>
        /// <param name="instrumentType">The type of instrument to throttle the results for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        public void ThrottleTopSymbols(string broker, InstrumentType instrumentType =
                                       InstrumentType.NoInstrument, int throttleRate = 0)
        {
            string destination = ParsedDestination.CreateTopSymbolsDestination(
                broker, TopSymbolsSubscription.InstrumentTypeToString(instrumentType)).Destination;

            Throttle(destination, throttleRate);
        }

        /// <summary>
        /// Throttles the stream with the given destination.
        /// </summary>
        /// <param name="destination">The destination of the stream to throttle.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        private void Throttle(string destination, int throttleRate)
        {
            Enqueue(() =>
            {
                // Make sure there is a leading slash.
                if (!destination.StartsWith("/"))
                    destination = '/' + destination;

                // If the destination exists, unsubscribe.
                if (_subscriptionsByDestination.TryGetValue(destination, out var subscription))
                    subscription.ThrottleRate = (uint)throttleRate;
            });
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Cleans the compression string so that it is normalized.
        /// </summary>
        /// <param name="compression">The compression string to clean.</param>
        /// <returns>The clean compression string.</returns>
        private static string CleanCompression(string compression)
        {
            // If no string, just return null.
            if (string.IsNullOrEmpty(compression))
                return string.Empty;

            // Put in lower-case and get rid of any whitespace.
            return compression.ToLower().Replace(" ", "");
        }

        /// <summary>
        /// Creates a destination string for a gauge subscription.
        /// </summary>
        /// <param name="path">The type of subscription to create a destination path for.</param>
        /// <param name="symbol">The symbol to get the destination path for.</param>
        /// <param name="compression">The compression to include in the destination path.</param>
        /// <returns>The standard format destination for the gauge.</returns>
        private string GetGaugeDestination(SubscriptionPath path, string symbol, string compression = "")
        {
            return ParsedDestination.CreateGaugeDestination(
                        path, ParsedDestination.StreamIDForSymbol(_streamID, symbol),
                        symbol, CleanCompression(compression)).Destination;
        }

        #endregion

        #region Timer Handling

        /// <summary>
        /// Handles timer events.
        /// </summary>
        /// <param name="state">State object (not used).</param>
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
                            // If we need to connect, reconnect.
                            Connect();
                            _reconnectTicks = 0;
                            _killTicks = utcTicks + _connectKill;
                        }
                        else if (utcTicks > _killTicks && _killTicks != 0)
                        {
                            // If we need to kill the connection, kill it.
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

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // Dispose of and clear the timer reference.
                _timer.Dispose();

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
