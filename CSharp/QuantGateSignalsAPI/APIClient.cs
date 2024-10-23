using Epcylon.APIs.Account;
using Epcylon.Common.Logging;
using Epcylon.Common.Net.NetCore.Client;
using Epcylon.Common.Net.ProtoStomp.Proto;
using Epcylon.Common.Net.Transport;
using Google.Protobuf;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.ProtoStomp;
using QuantGate.API.Signals.Subscriptions;
using QuantGate.API.Signals.Utilities;
using System.Collections.Concurrent;
using EVENTS = QuantGate.API.Signals.Events;

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
        public event EventHandler<EVENTS.ErrorEventArgs> Error = delegate { };

        public event EventHandler<PricesEventArgs> PricesUpdated = delegate { };
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
        private readonly Dictionary<ulong, ProtoStompSubscription> _subscriptionReferences = [];
        /// <summary>
        /// Holds a list of all current subscriptions by destination.
        /// </summary>
        private readonly Dictionary<string, ProtoStompSubscription> _subscriptionsByDestination = [];

        /// <summary>
        /// Holds a list of all requests requiring a receipt.
        /// </summary>
        private readonly Dictionary<ulong, IReceiptable> _receiptReferences = [];

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
        /// Connection body to use (internal).
        /// </summary>
        private readonly string _connectBody;

        /// <summary>
        /// The password for the current connection (JWT Token).
        /// </summary>        
        private readonly ConnectionToken _token;

        /// <summary>
        /// Is the client disconnecting?
        /// </summary>
        private bool _isDisconnecting = false;
        /// <summary>
        /// Is the disconnection a full disconnect?
        /// </summary>
        private bool _fullDisconnect = false;
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
        /// The maximum time to wait before receiving a message.
        /// </summary>
        private const long _maxHeartBeatWait = 1 * TimeSpan.TicksPerMinute;
        /// <summary>
        /// Time to wait after not receiving a message before sending a heartbeat request.
        /// </summary>
        private const long _heartBeatCheckTicks = 10 * TimeSpan.TicksPerSecond;
        /// <summary>
        /// The last time that a message was sent.
        /// </summary>
        private long _lastSentTicks = 0;
        /// <summary>
        /// The last time that a message was received.
        /// </summary>
        private long _lastReceivedTicks = 0;

        #endregion

        #region Private and Internal Variables

        /// <summary>
        /// The dispatcher to use for threading.
        /// </summary>
        internal SynchronizationContext Sync { get; }

        /// <summary>
        /// Blocking message queue used in the main thread to process new actions within the thread.
        /// </summary>
        private readonly BlockingCollection<Action> _actions = [];

        /// <summary>
        /// Client transport layer interface instance.
        /// </summary>
        private ITransport<byte[]> _client;

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
        /// <param name="stream">The base data stream to connect to (default = realtime).</param>
        /// <param name="sync">
        /// The synchronization context to return values on (default = SynchronizationContext.Current).
        /// </param>
        /// <param name="connectBody">Connect body to send (internal use only).</param>
        public APIClient(ConnectionToken token, DataStream stream = DataStream.Realtime,
                         SynchronizationContext sync = null, string connectBody = null)
        {
            // Set the connect body.
            _connectBody = connectBody;
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
            if (stream == DataStream.Invalid)
                Stream = DataStream.Realtime;
            else
                Stream = stream;

            _streamID = ConvertStream(Stream);

            // Start the main (long running) thread queue.
            Task.Factory.StartNew(HandleActions, TaskCreationOptions.LongRunning);

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
        internal void Enqueue(Action action)
        {
            try
            {
                // Try to enqueue the action.
                if (!_actions.IsAddingCompleted)
                    _actions.Add(action);
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":NQ", ex);
            }
        }

        /// <summary>
        /// This is the main thread loop. Handles actions from a blocking collection until cancelled.
        /// </summary>
        private void HandleActions()
        {
            try
            {
                // Go through each action in the blocking collection while not cancelled.
                foreach (Action action in _actions.GetConsumingEnumerable())
                    action();
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":HActs", ex);
            }
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

                    if (_token is not null)
                    {
                        connect.Login = _token.ClientID;
                        connect.Passcode = _token.Token;

                        if (!string.IsNullOrEmpty(_connectBody))
                            connect.Body = ByteString.FromBase64(_connectBody);
                    }

                    Send(new RequestFrame { Connect = connect });
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException($"{_moduleID}:OO", ex);
                }
            });
        }

        private void OnClose(object source, EventArgs args)
        {
            Enqueue(() =>
            {
                try
                {
                    if (_fullDisconnect || _isDisposed)
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

                    // We're disconnected, clear the client reference.
                    ClearConnection();

                    SharedLogger.LogDebug($"{_moduleID}:OCl", "API Client was disconnected.");
                    Sync.Post(new SendOrPostCallback(o => { Disconnected(this, EventArgs.Empty); }), null);

                    // If disposed, stop the thread.
                    if (_isDisposed)
                        _actions.CompleteAdding();
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException($"{_moduleID}:OCl", ex);
                    // We're disconnected, clear the client reference.
                    ClearConnection();
                }
            });
        }

        /// <summary>
        /// Dispatches the given message to a registered message consumer.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        private void HandleMessage(object source, byte[] message)
        {
            Enqueue(() =>
            {
                ResponseFrame frame;

                try
                {
                    // Mark as the last time a message was received.
                    _lastReceivedTicks = DateTime.UtcNow.Ticks;

                    // Parse the next message frame.
                    frame = ResponseFrame.Parser.ParseFrom(message);

                    // If parsed properly and the consumer can be found, call the consumer for the message.
                    if ((frame is not null) && _messageConsumers.TryGetValue(frame.ResponseCase, out var consumer))
                        consumer(frame);
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException($"{_moduleID}:HMsg", ex);
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

                SharedLogger.LogDebug(_moduleID + ":OSCn", "API Client was connected.");
                Sync.Post(new SendOrPostCallback(o => { Connected(this, EventArgs.Empty); }), null);
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":OSCn", ex);
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
                SharedLogger.LogException(_moduleID + ":HMF", ex);
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
                SharedLogger.LogException(_moduleID + ":HMsF", ex);
            }
        }

        private void HandleMessage(MessageResponse message)
        {
            try
            {
                // If the subscription id exists, try to get the subscription.                
                _subscriptionReferences.TryGetValue(message.SubscriptionId, out ProtoStompSubscription subscription);

                if (subscription is not null)
                {
                    // If the subscription was found, handle the next message.
                    (subscription as IObserver<ByteString>).OnNext(message.Body);
                }
                else if (!_isDisconnecting && IsConnected)
                {
                    // If not disconnecting, log an error.
                    SharedLogger.LogDebug(_moduleID + ":HM", "Subscription not found", "Id={Id}",
                                          message.SubscriptionId.ToString());
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":HM", ex);
            }
        }

        private void HandleReceiptFrame(ResponseFrame frame)
        {
            try
            {
                // Try to get the receiptable and remove if found.
                if (_receiptReferences.TryGetValue(frame.Receipt.ReceiptId, out IReceiptable receiptable))
                    _receiptReferences.Remove(frame.Receipt.ReceiptId);

                if (receiptable is not null)
                {
                    receiptable.OnReceipt();
                }
                else if (!_isDisconnecting && IsConnected)
                {
                    SharedLogger.LogError(_moduleID + ":HRF", "Receiptable not found for id",
                                          "Receipt={Receipt}", frame.Receipt.ReceiptId.ToString());
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":HRF", ex);
            }
        }

        private void HandleErrorFrame(ResponseFrame frame)
        {
            Sync.Post(new SendOrPostCallback(o => { Error(this, new EVENTS.ErrorEventArgs(frame.ServerError.Message)); }), null);
        }

        private void HandleSubscriptionError(ResponseFrame frame)
        {
            try
            {
                // If the subscription id exists, try to get the subscription.
                _subscriptionReferences.TryGetValue(
                    frame.SubscriptionError.SubscriptionId, out ProtoStompSubscription subscription);

                if (subscription is not null)
                {
                    // If the subscription was found, handle the error. 
                    (subscription as IObserver<ByteString>).
                        OnError(new Exception(frame.SubscriptionError.Message,
                                              new Exception(frame.SubscriptionError.Details)));
                }
                else if (!_isDisconnecting && IsConnected)
                {
                    // If not disconnecting, log an error.
                    SharedLogger.LogDebug(_moduleID + ":HSE", "Subscription not found", "Id={Id}",
                                          frame.SubscriptionError.SubscriptionId.ToString());
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":HSE", ex);
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
            _lastReceivedTicks = DateTime.UtcNow.Ticks;
        }

        #endregion        

        #region Connection Handling        

        /// <summary>
        /// The stomp client to connect with.
        /// </summary>
        private ITransport<byte[]> Client
        {
            get { return _client; }
            set
            {
                if (!ReferenceEquals(_client, value))
                {
                    // If the client changed, remove old event handlers.
                    if (_client is not null)
                    {
                        // Remove old handlers for the connected and disconnected events.
                        _client.OnOpen -= OnOpen;
                        _client.OnClose -= OnClose;
                        _client.OnMessage -= HandleMessage;
                        // Close the client (if not already closed).
                        _client.Close();
                    }

                    // Set the new client.
                    _client = value;

                    if (_client is not null)
                    {
                        // Add new handlers for the connected and disconnected events.
                        _client.OnOpen += OnOpen;
                        _client.OnClose += OnClose;
                        _client.OnMessage += HandleMessage;
                    }
                }
            }
        }

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
                    _fullDisconnect = false;
                    _reconnectTicks = 0;
                    _killTicks = DateTime.UtcNow.Ticks + _connectKill;

                    SharedLogger.LogDebug(_moduleID + ":Cn2", "Connecting API client.");

                    // Create the new WebSocket and connect.
                    Client = new WebsocketBinaryTransport(new Uri($"{_host}:{_port}/"));
                    Client.Connect();
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":Cn2", ex);
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
        /// Disconnect from the WebSocket server.
        /// </summary>
        /// <param name="full">True if this is a full disconnect.</param>
        private void Disconnect(bool full)
        {
            try
            {
                SharedLogger.LogDebug(_moduleID + ":DCn", "Disconnecting API client.");

                if (full)
                {
                    // If doing a full disconnect, set to full disconnect.
                    _fullDisconnect = true;
                    // Clear the subscriptions.
                    ClearSubscriptions();
                }

                // Send disconnect frame, if the transport is alive.
                Send(new RequestFrame { Disconnect = new DisconnectRequest() });

                if (Client is not null)
                {
                    // If there is a client, 
                    if (Client.IsConnected)
                    {
                        // We're disconnecting now.
                        _isDisconnecting = true;
                        // Close if not closed.
                        Client.Close();
                    }
                    else
                    {
                        // We're disconnected, clear the client reference.
                        ClearConnection();
                        OnClose(this, EventArgs.Empty);
                    }
                }
                else
                {
                    // If no client, clear the connection.
                    ClearConnection();
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":Cls", ex);
                // We're disconnected, clear the client reference.
                ClearConnection();
                OnClose(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Clears the connection state and sets to disconnected.
        /// </summary>
        private void ClearConnection()
        {
            IsConnected = false;
            _isDisconnecting = false;
            Client = null;
        }

        /// <summary>
        /// Sends the given request frame to the WebSocket endpoint.
        /// </summary>
        /// <param name="frame">The frame to send.</param>
        private void Send(RequestFrame frame)
        {
            try
            {
                // If we're in a state to send, send the message.
                if (Client is not null && Client.IsConnected && !_isDisconnecting)
                {
                    Client.Send(frame.ToByteArray());
                    _lastSentTicks = DateTime.UtcNow.Ticks;
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":Snd", ex);
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
                if (subscription is not null)
                {
                    _subscriptionReferences[subscription.SubscriptionID] = subscription;
                    _subscriptionsByDestination[subscription.Destination] = subscription;

                    if (subscription.ReceiptID != 0)
                        _receiptReferences.Add(subscription.ReceiptID, subscription);

                    if (IsConnected)
                    {
                        // If connected, send the subscription request - otherwise, waiting for connection.
                        Send(new RequestFrame { Subscribe = subscription.Request });

                        // Log the subscription action.
                        SharedLogger.LogDebug(_moduleID + ":Sub", "Subscribe",
                                                "Destination={Destination}, SubscriptionId={SubscriptionId}",
                                                subscription.Destination, subscription.SubscriptionID.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":Sub", ex);
            }
        }

        /// <summary>
        /// Throttles the specified subscription.
        /// </summary>
        internal void Throttle(ProtoStompSubscription subscription, uint rate)
        {
            ThrottleRequest throttle = new();
            ProtoStompReceipt receipt = new(IDGenerator.NextID);

            try
            {
                if (subscription is not null)
                {
                    // Create the unsubscribe message.
                    throttle.SubscriptionId = subscription.SubscriptionID;
                    throttle.ThrottleRate = rate;
                    throttle.ReceiptId = receipt.ReceiptID;

                    // Add to the receiptable requests.                        
                    _receiptReferences.Add(receipt.ReceiptID, receipt);

                    if (IsConnected)
                    {
                        // If connected, send the throttle request - if not sent, will be applied to the initial subscription.
                        Send(new RequestFrame { Throttle = throttle });

                        // Log the throttle action.
                        SharedLogger.LogDebug(_moduleID + ":Thr", "Throttle",
                                              "Destination={Destination}, SubscriptionId={SubscriptionId}, Rate={Rate}",
                                              subscription.Destination, subscription.SubscriptionID.ToString(), rate.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":Thr", ex);
            }
        }

        /// <summary>
        /// Unsubscribes the specified subscription.
        /// </summary>
        /// <param name="subscription">The subscription to unsubscribe from.</param>
        internal void Unsubscribe(ProtoStompSubscription subscription, object reference = null)
        {
            ProtoStompReceipt receipt = new(IDGenerator.NextID);
            UnsubscribeRequest unsubscribe = new();

            try
            {
                // Nothing to do if no subscription.
                if (subscription is not SubscriptionBase converted)
                    return;

                lock (converted._references)
                {
                    // Remove appropriate references.
                    if (reference is null)
                        converted._references.Clear();
                    else
                        converted._references.Remove(reference);

                    // If there are still references, don't remove until last is gone.
                    if (converted._references.Count > 0)
                        return;
                }

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

                if (IsConnected)
                {
                    // If connected, send the message.
                    Send(new RequestFrame { Unsubscribe = unsubscribe });

                    // Log the subscription action.
                    SharedLogger.LogDebug(_moduleID + ":USub", "Unsubscribe",
                                          "Destination={Destination}, SubscriptionId={SubscriptionId}",
                                          subscription.Destination, subscription.SubscriptionID.ToString());
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":USub", ex);
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

                if (IsConnected)
                {
                    // Send the Send request.
                    Send(new RequestFrame { Send = toSend.Request });
                }
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":Snd", ex);
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
                subscriptions = [.. _subscriptionReferences.Values];
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
                SharedLogger.LogException(_moduleID + ":ReSub", ex);
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
                subscriptions = [.. _subscriptionReferences.Values];
                _subscriptionReferences.Clear();
                _subscriptionsByDestination.Clear();
                _search = null;

                // Handle OnCompleted events for each subscription.
                foreach (IObserver<ByteString> subscription in subscriptions)
                    subscription.OnCompleted();

                // Clear the receipt references.
                receiptables = [.. _receiptReferences.Values];
                _receiptReferences.Clear();

                // Handle the OnInvalidate events for each subscription.
                foreach (IReceiptable receiptable in receiptables)
                    receiptable.OnInvalidate();
            }
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":ClrSs", ex);
            }
        }

        #endregion

        #region Subscription Creation Methods

        /// <summary>
        /// Subscribes to a price data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the prices for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>    
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribePrices(string symbol, int throttleRate = 0,
                                    object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new PricesSubscription(this, SendPricesUpdate, ConvertStream(stream), symbol,
                                                       false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Perception gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Perception data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribePerception(string symbol, int throttleRate = 0,
                                        object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new PerceptionSubscription(this, SendPerceptionUpdate, ConvertStream(stream), symbol,
                                                           false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Commitment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Commitment data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeCommitment(string symbol, int throttleRate = 0,
                                        object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new CommitmentSubscription(this, SendCommitmentUpdate, ConvertStream(stream), symbol,
                                                           false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Book Pressure gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Book Pressure data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeBookPressure(string symbol, int throttleRate = 0,
                                          object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new BookPressureSubscription(this, SendBookPressureUpdate, ConvertStream(stream),
                                                             symbol, false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Headroom gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Headroom data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeHeadroom(string symbol, int throttleRate = 0,
                                      object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new HeadroomSubscription(this, SendHeadroomUpdate, ConvertStream(stream), symbol,
                                                         false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Sentiment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Sentiment data for.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 50t.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeSentiment(string symbol, string compression = "50t", int throttleRate = 0,
                                       object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new SentimentSubscription(this, SendSentimentUpdate, ConvertStream(stream), symbol,
                                                          CleanCompression(compression), false,
                                                          (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Equilibrium data for.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 300s.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeEquilibrium(string symbol, string compression = "300s", int throttleRate = 0,
                                         object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new EquilibriumSubscription(this, SendEquilibriumUpdate, ConvertStream(stream), symbol,
                                                            CleanCompression(compression), false,
                                                            (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Multiframe Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Multiframe Equilibrium data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeMultiframeEquilibrium(string symbol, int throttleRate = 0,
                                                   object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new MultiframeSubscription(this, SendMultiframeEquilibriumUpdate, ConvertStream(stream),
                                                           symbol, false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Trigger gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">Symbol to get the Trigger data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeTrigger(string symbol, int throttleRate = 0,
                                     object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new TriggerSubscription(this, SendTriggerUpdate, ConvertStream(stream), symbol,
                                                        false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a Strategy update data stream for a specific strategy and symbol.
        /// </summary>
        /// <param name="strategyID">Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.</param>
        /// <param name="symbol">Symbol to get the Strategy update data for.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeStrategy(string strategyID, string symbol, int throttleRate = 0,
                                      object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new StrategySubscription(this, SendStrategyUpdate, strategyID, ConvertStream(stream),
                                                         symbol, false, (uint)throttleRate, reference));

        /// <summary>
        /// Subscribes to a stream of Top Symbols according to broker and instrument type.
        /// </summary>
        /// <param name="broker">The broker to get the Top Symbols for. Must match a valid broker type string.</param>
        /// <param name="instrumentType">The type of instrument to include in the results.</param>
        /// <param name="throttleRate">Rate to throttle messages at (in ms). Enter 0 for no throttling.</param>
        /// <returns>The Top Symbols data object that will receive the updates.</returns>
        /// <param name="reference">Reference to return with the event.</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void SubscribeTopSymbols(string broker, InstrumentType instrumentType =
                                        InstrumentType.NoInstrument, int throttleRate = 0, object reference = null) =>
            EnqueueAndSubscribe(new TopSymbolsSubscription(this, SendTopSymbolsUpdate, broker, instrumentType,
                                                           false, (uint)throttleRate, reference));

        /// <summary>
        /// Used to retrieve the specification details of an instrument according to its symbol,
        /// as listed by the QuantGate servers.
        /// </summary>        
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void RequestInstrumentDetails(string symbol, object reference = null,
                                             DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new InstrumentSubscription(this, SendInstrumentUpdate, ConvertStream(stream),
                                                           symbol, reference: reference));

        /// <summary>
        /// Used to retrieve a list of futures for the given underlying and currency, as listed by the QuantGate servers.
        /// </summary>
        /// <param name="underlying">The underlying of the futures to retrieve.</param>
        /// <param name="currency">The currency of the futures to retrieve.</param>
        /// <param name="reference">Reference to return with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        /// <remarks>
        /// Note that if two calls are made to this function with different references, 
        /// you will receive two events on each update - one for each reference supplied.
        /// If no reference is supplied, you will receive a single update.
        /// </remarks>
        public void RequestFutures(string underlying, string currency,
                                   object reference = null, DataStream stream = DataStream.Invalid) =>
            EnqueueAndSubscribe(new FuturesListSubscription(this, SendFuturesListUpdate, ConvertStream(stream),
                                                            underlying, currency, reference: reference));

        /// <summary>
        /// Enqueues and subscribes to a stream.
        /// </summary>
        /// <param name="subscription">The subscription to subscribe to.</param>
        private void EnqueueAndSubscribe<M, V>(SubscriptionBase<M, V> subscription)
            where M : IMessage<M>
            where V : SubscriptionEventArgs
        {
            Enqueue(() =>
            {
                try
                {
                    if (_subscriptionsByDestination.TryGetValue(subscription.Destination, out ProtoStompSubscription existing))
                    {
                        if (existing is SubscriptionBase converted)
                        {
                            // If already subscribed, check for subscription reference.
                            IReadOnlyList<object> references = subscription.References;

                            if (references.Count > 0)
                            {
                                // If there are references to add, add the reference.
                                lock (converted._references)
                                    converted._references.Add(references[0]);
                            }
                        }
                    }
                    else
                    {
                        // If not yet subscribed, set the parent event handler and subscribe.
                        Subscribe(subscription);
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":NQnSubs", ex);
                }
            });
        }

        /// <summary>
        /// Requests symbols that match a specific term and (optionally) a specific broker. 
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <param name="broker">Broker to get the results for. If supplied, must match a valid broker type string.</param>
        /// <param name="reference">Reference to return with the event.</param>
        public void SearchSymbols(string term, string broker, object reference = null,
                                  DataStream stream = DataStream.Invalid)
        {
            Enqueue(() =>
            {
                try
                {
                    if (_search is null)
                    {
                        _search = new SearchSubscription(this, SendSymbolSearchUpdate,
                                                         ConvertStream(stream), reference: reference);
                        EnqueueAndSubscribe(_search);
                    }

                    _search.Search(term, broker);
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":SSyms", ex);
                }
            });
        }

        #endregion

        #region Unsubscription Methods

        /// <summary>
        /// Unsubscribes from a prices data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting prices for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribePrices(string symbol, object reference = null,
                                      DataStream stream = DataStream.Invalid) =>
            Unsubscribe(ParsedDestination.CreatePricesDestination(
                ParsedDestination.StreamIDForSymbol(ConvertStream(stream), symbol), symbol).Destination, reference);

        /// <summary>
        /// Unsubscribes from a Perception gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Perception data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribePerception(string symbol, object reference = null,
                                          DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugePerception, symbol, stream: stream), reference);

        /// <summary>
        /// Unsubscribes from a Commitment gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Commitment data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeCommitment(string symbol, object reference = null,
                                          DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeCommitment, symbol, "1m", stream), reference);

        /// <summary>
        /// Unsubscribes from a Book Pressure gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Book Pressure data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeBookPressure(string symbol, object reference = null,
                                            DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeBookPressure, symbol, "0q", stream), reference);

        /// <summary>
        /// Unsubscribes from a Headroom gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Headroom data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeHeadroom(string symbol, object reference = null,
                                        DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeHeadroom, symbol, "5m", stream), reference);

        /// <summary>
        /// Unsubscribes from a Sentiment gauge data stream at the specified compression for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Sentiment data for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 50t.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeSentiment(string symbol, string compression = "50t",
                                         object reference = null, DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeSentiment, symbol, compression, stream), reference);

        /// <summary>
        /// Unsubscribes from an Equilibrium gauge data stream at the specified compression for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Equilibrium data for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 300s.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeEquilibrium(string symbol, string compression = "300s",
                                           object reference = null, DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeEquilibrium, symbol, compression, stream), reference);

        /// <summary>
        /// Unsubscribes from a Multiframe Equilibrium gauge data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Multiframe Equilibrium data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeMultiframeEquilibrium(string symbol, object reference = null,
                                                     DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeMultiframeEquilibrium, symbol, stream: stream), reference);

        /// <summary>
        /// Unsubscribes from a Trigger data stream for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to stop getting Triger data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeTrigger(string symbol, object reference = null, DataStream stream = DataStream.Invalid) =>
            Unsubscribe(GetGaugeDestination(SubscriptionPath.GaugeTrigger, symbol, stream: stream), reference);

        /// <summary>
        /// Unsubscribes from Strategy data for the given strategy and symbol.
        /// </summary>
        /// <param name="strategyID">The identifier of the strategy to stop running.</param>
        /// <param name="symbol">The symbol to stop getting Strategy data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void UnsubscribeStrategy(string strategyID, string symbol,
                                        object reference = null, DataStream stream = DataStream.Invalid)
        {
            string destination = ParsedDestination.CreateStrategyDestination(
                strategyID, ParsedDestination.StreamIDForSymbol(ConvertStream(stream), symbol), symbol).Destination;

            Unsubscribe(destination, reference);
        }

        /// <summary>
        /// Unsubscribes from Top Symbols data for the given broker and instrument type.
        /// </summary>
        /// <param name="broker">The broker to stop getting the Top Symbols for.</param>
        /// <param name="instrumentType">The type of instrument to stop including from the results.</param>        
        /// <param name="reference">Reference returned with the event.</param>
        public void UnsubscribeTopSymbols(string broker, InstrumentType instrumentType =
                                          InstrumentType.NoInstrument, object reference = null)
        {
            string destination = ParsedDestination.CreateTopSymbolsDestination(
                broker, TopSymbolsSubscription.InstrumentTypeToString(instrumentType)).Destination;

            Unsubscribe(destination, reference);
        }

        /// <summary>
        /// Used to unsubscribe from a stream of data with the destination supplied.
        /// </summary>
        /// <param name="destination">The destination of the stream to stop getting data for.</param>
        /// <param name="reference">Reference returned with the event.</param>
        private void Unsubscribe(string destination, object reference)
        {
            Enqueue(() =>
            {
                try
                {
                    // Make sure there is a leading slash.
                    if (!destination.StartsWith('/'))
                        destination = '/' + destination;

                    // If the destination exists, unsubscribe (otherwise, no need).
                    if (_subscriptionsByDestination.TryGetValue(destination, out var subscription))
                        Unsubscribe(subscription, reference);
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":USub", ex);
                }
            });
        }

        /// <summary>
        /// Unsubscribes from all active subscriptions.
        /// </summary>
        public void UnsubscribeAll()
        {
            Enqueue(() =>
            {
                try
                {
                    // Get a list of all of the current subscriptions.
                    List<ProtoStompSubscription> subscriptions = [.. _subscriptionReferences.Values];

                    // If no symbol supplied, unsubscribe from everything.
                    foreach (ProtoStompSubscription subscription in subscriptions)
                        Unsubscribe(subscription);

                    // This includes the search.
                    _search = null;
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":USubA", ex);
                }
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
                try
                {
                    // Get a list of all of the current subscriptions.
                    List<ProtoStompSubscription> subscriptions = [.. _subscriptionReferences.Values];

                    // If a symbol was supplied, need to check each subscription.
                    foreach (ProtoStompSubscription subscription in subscriptions)
                    {
                        // If the subscription is tied to the symbol, unsubscribe.
                        if ((subscription is ISymbolSubscription symbolSubscription) &&
                            (symbolSubscription.Symbol == symbol))
                            Unsubscribe(subscription);
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":USubA-s", ex);
                }
            });
        }

        #endregion

        #region Subscription Throttling Methods

        /// <summary>
        /// Changes the maximum rate at which the back-end sends price updates for the given symbol.        
        /// </summary>
        /// <param name="symbol">The symbol to change the price throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottlePrices(string symbol, int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(ParsedDestination.CreatePricesDestination(
                ParsedDestination.StreamIDForSymbol(ConvertStream(stream), symbol), symbol).Destination, throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Perception gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Perception gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottlePerception(string symbol, int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugePerception, symbol, stream: stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Commitment gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Commitment gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleCommitment(string symbol, int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeCommitment, symbol, "1m", stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Book Pressure gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Book Pressure gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleBookPressure(string symbol, int throttleRate = 0,
                                         DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeBookPressure, symbol, "0q", stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Headroom gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Headroom gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleHeadroom(string symbol, int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeHeadroom, symbol, "5m", stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Sentiment gauge updates for a 
        /// specific symbol at the specified compression.
        /// </summary>
        /// <param name="symbol">The symbol to change the Sentiment gauge throttle rate for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 50t.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleSentiment(string symbol, string compression = "50t",
                                      int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeSentiment, symbol, compression, stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Equilibrium gauge updates for a 
        /// specific symbol at the specified compression.
        /// </summary>
        /// <param name="symbol">The symbol to change the Equilibrium gauge throttle rate for.</param>
        /// <param name="compression">Compression timeframe being applied to the gauge. Default value is 300s.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleEquilibrium(string symbol, string compression = "300s",
                                        int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeEquilibrium, symbol, compression, stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Multiframe Equilibrium gauge updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Multiframe Equilibrium gauge throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleMultiframeEquilibrium(string symbol, int throttleRate = 0,
                                                  DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeMultiframeEquilibrium, symbol, stream: stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Trigger updates for the given symbol.
        /// </summary>
        /// <param name="symbol">The symbol to change the Trigger throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleTrigger(string symbol, int throttleRate = 0, DataStream stream = DataStream.Invalid) =>
            Throttle(GetGaugeDestination(SubscriptionPath.GaugeTrigger, symbol, stream: stream), throttleRate);

        /// <summary>
        /// Changes the maximum rate at which the back-end sends Strategy updates for the 
        /// given strategy and symbol.
        /// </summary>
        /// <param name="strategyID">The identifier of the strategy to throttle.</param>
        /// <param name="symbol">The symbol to change the Strategy throttle rate for.</param>
        /// <param name="throttleRate">The new throttle rate to set to (in ms). Enter 0 for no throttling.</param>
        /// <param name="stream">Stream to request for (overrides stream supplied on connection, if supplied).</param>
        public void ThrottleStrategy(string strategyID, string symbol,
                                     int throttleRate = 0, DataStream stream = DataStream.Invalid)
        {
            string destination = ParsedDestination.CreateStrategyDestination(
                strategyID, ParsedDestination.StreamIDForSymbol(ConvertStream(stream), symbol), symbol).Destination;

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
                try
                {
                    // Make sure there is a leading slash.
                    if (!destination.StartsWith('/'))
                        destination = '/' + destination;

                    // If the destination exists, unsubscribe.
                    if (_subscriptionsByDestination.TryGetValue(destination, out var subscription))
                        subscription.ThrottleRate = (uint)throttleRate;
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":Thr", ex);
                }
            });
        }

        #endregion

        #region Subscription Event Sending Methods

        /// <summary>
        /// Sends a prices update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendPricesUpdate(object sender, PricesEventArgs e) => PricesUpdated(sender, e);
        /// <summary>
        /// Sends a perception update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendPerceptionUpdate(object sender, PerceptionEventArgs e) => PerceptionUpdated(sender, e);
        /// <summary>
        /// Sends a commitment update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendCommitmentUpdate(object sender, CommitmentEventArgs e) => CommitmentUpdated(sender, e);
        /// <summary>
        /// Sends a equilibrium update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendEquilibriumUpdate(object sender, EquilibriumEventArgs e) => EquilibriumUpdated(sender, e);
        /// <summary>
        /// Sends a sentiment update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendSentimentUpdate(object sender, SentimentEventArgs e) => SentimentUpdated(sender, e);
        /// <summary>
        /// Sends a book pressure update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendBookPressureUpdate(object sender, BookPressureEventArgs e) => BookPressureUpdated(sender, e);
        /// <summary>
        /// Sends a headroom update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendHeadroomUpdate(object sender, HeadroomEventArgs e) => HeadroomUpdated(sender, e);
        /// <summary>
        /// Sends a multi-frame equilibrium update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendMultiframeEquilibriumUpdate(object sender, MultiframeEquilibriumEventArgs e) => MultiframeEquilibriumUpdated(sender, e);
        /// <summary>
        /// Sends a trigger update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendTriggerUpdate(object sender, TriggerEventArgs e) => TriggerUpdated(sender, e);
        /// <summary>
        /// Sends a symbol search update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendSymbolSearchUpdate(object sender, SearchResultsEventArgs e) => SymbolSearchUpdated(sender, e);
        /// <summary>
        /// Sends a top symbols update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendTopSymbolsUpdate(object sender, TopSymbolsEventArgs e) => TopSymbolsUpdated(sender, e);
        /// <summary>
        /// Sends a strategy update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendStrategyUpdate(object sender, StrategyEventArgs e) => StrategyUpdated(sender, e);
        /// <summary>
        /// Sends a instrument update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendInstrumentUpdate(object sender, InstrumentEventArgs e) => InstrumentUpdated(sender, e);
        /// <summary>
        /// Sends a futures list update to the event handlers.
        /// </summary>
        /// <remarks>Cannot send the event handler reference as this will change when handlers get added.</remarks>
        private void SendFuturesListUpdate(object sender, FuturesListEventArgs e) => FuturesListUpdated(sender, e);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Converts a data stream to a stream ID. If "Invalid" is used, will use the default stream.
        /// </summary>
        /// <param name="stream">The data stream to convert.</param>
        /// <returns>The stream ID for the data stream.</returns>
        private string ConvertStream(DataStream stream)
        {
            if (stream == DataStream.Invalid)
                return _streamID;
            else
                return ToString(stream);
        }

        /// <summary>
        /// Converts a stream or api type id to a DataStream value.
        /// </summary>
        /// <param name="id">The stream or api type id to convert.</param>
        /// <returns>The DataStream value associated with the id.</returns>
        public static DataStream ToStream(string id)
        {
            return id.ToLower() switch
            {
                ParsedDestination.RealtimeStreamID or "rt_paper" => DataStream.Realtime,
                ParsedDestination.DemoStreamID => DataStream.Demo,
                _ => DataStream.Delayed,
            };
        }

        /// <summary>
        /// Converts a data stream to a stream ID. If "Invalid" is used, will use the default stream.
        /// </summary>
        /// <param name="stream">The data stream to convert.</param>
        /// <returns>The stream ID for the data stream.</returns>
        public static string ToString(DataStream stream)
        {
            return stream switch
            {
                DataStream.Realtime => ParsedDestination.RealtimeStreamID,
                DataStream.Demo => ParsedDestination.DemoStreamID,
                _ => ParsedDestination.DelayStreamID,
            };
        }

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
        private string GetGaugeDestination(SubscriptionPath path, string symbol, string compression = "",
                                           DataStream stream = DataStream.Invalid)
        {
            return ParsedDestination.CreateGaugeDestination(
                        path, ParsedDestination.StreamIDForSymbol(ConvertStream(stream), symbol),
                        symbol, CleanCompression(compression)).Destination;
        }

        /// <summary>
        /// Prints the current status.
        /// </summary>
        public void PrintStatus()
        {
            Enqueue(() =>
            {
                List<string> subscriptions;

                try
                {
                    // Get the runners and sort.
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("Feed status: " + (IsConnected ? "Connected" : "Disconnected"));
                    Console.WriteLine("-----------------------------");

                    subscriptions = [.. _subscriptionsByDestination.Keys];
                    subscriptions.Sort();

                    foreach (string subscription in subscriptions)
                        Console.WriteLine(subscription);

                    Console.WriteLine("-----------------------------");
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":PSt", ex);
                }
            });
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

                    if (!IsConnected)
                    {
                        // If not connected, check if we need to reconnect.
                        if (!_isDisconnecting)
                        {
                            // Don't connect or disconnect while disconnecting.
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
                    }
                    else if (IsConnected)
                    {
                        // If connected and not disconnecting.
                        if (utcTicks > _lastReceivedTicks + 2 * _maxHeartBeatWait)
                        {
                            // If more than two times the regular heartbeat timeout, force a full close.
                            // At this point, it's not closing properly, so we want to initiate a new
                            // connection without regard to how well the current instance is closing.
                            OnClose(this, EventArgs.Empty);
                        }
                        else if (!_isDisconnecting && utcTicks > _lastReceivedTicks + _maxHeartBeatWait)
                        {
                            // If it's been too long before receiving a message, disconnect (to reconnect).
                            Disconnect(false);
                        }
                        else if (utcTicks > _lastSentTicks + _heartBeatCheckTicks)
                        {
                            // If past the last heartbeat checks, request a heartbeat.
                            Send(new RequestFrame { Heartbeat = new Heartbeat() });
                        }
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.LogException(_moduleID + ":Tmr", ex);
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
            try
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
            catch (Exception ex)
            {
                SharedLogger.LogException(_moduleID + ":Dsp", ex);
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
