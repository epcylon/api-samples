using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using QuantGate.API.ProtoStomp;
using QuantGate.API.Subscriptions;
using QuantGate.API.Utilities;
using QuantGate.API.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;

namespace QuantGate.API
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

        /// <summary>
        /// Transport layer interface instance.
        /// </summary>
        private readonly WebSocket _transport;

        /// <summary>
        /// The Stomp version the client is connected to.
        /// </summary>
        public string StompVersion { get; private set; }

        public event Action<APIClient> Connected = delegate { };
        public event Action<APIClient> Disconnected = delegate { };
        public event Action<APIClient> OnHeartbeat = delegate { };
        public event Action<APIClient, string> Error = delegate { };

        /// <summary>
        /// The host address of the server to connect to.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The port of the server to connect to.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Stream ID associated with the stream the client is connected to.
        /// </summary>
        private string _streamID;
        public DataStream Stream { get; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        /// <summary>
        /// Is the client disconnecting?
        /// </summary>
        private bool _isDisconnecting = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="StompClient" /> class.
        /// </summary>
        /// <param name="host">The web address to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        public APIClient(string host, int port = int.MinValue, DataStream stream = DataStream.Realtime)
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

            // Create the new websocket.
            _transport = new WebSocket(Host + ':' + Port + "/");

            // Set up the event handling.
            _transport.OnOpen += (o, e) => OnOpen(o);
            _transport.OnClose += (o, e) => OnClose(o);
            _transport.OnMessage += (o, e) => HandleMessage(o, e.RawData);
            _transport.OnError += (o, e) => OnError(o, e);

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
        }

        private void OnError(object o, ErrorEventArgs e)
        {
            string message;

            message = e.Message + (e.Exception?.Message ?? string.Empty);

            // Handle the error message.
            Trace.TraceError(_moduleID + ":HE - Stomp transport error: " + message);

            // Make sure it closes properly.                        
            if ((_transport is object) && (_transport.ReadyState == WebSocketState.Closed))
                Close();
        }


        /// <summary>
        /// Used to close the transport object.
        /// </summary>
        void Close()
        {
            try
            {
                if (_transport is object)
                {
                    if (_transport.ReadyState != WebSocketState.Closed)
                        _transport.CloseAsync();
                    else
                        OnClose(this);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cls - " + ex.Message);
                OnClose(this);
            }
        }


        private void HandleSubscriptionError(ResponseFrame frame)
        {
            ProtoStompSubscription subscription;

            try
            {
                // If the subscription id exists, try to get the subscription.
                lock (_subscriptionReferences)
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

        private void HandleHeartbeatFrame(ResponseFrame frame)
        {
            try
            {
                // Handle as message.
                OnHeartbeat(this);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HHbF - " + ex.Message);
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
                lock (_subscriptionReferences)
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
            IReceiptable receiptable;

            try
            {
                // Try to get the receiptable and remove if found.
                lock (_receiptReferences)
                    if (_receiptReferences.TryGetValue(frame.Receipt.ReceiptId, out receiptable))
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
            Error(this, frame.ServerError.Message);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref = "APIClient" /> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Connects to the server on the specified address.
        /// </summary>
        /// <param name="jwtToken">Jwt Token to connect with.</param>
        public void Connect(string jwtToken)
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

                // Connect to the websocket.
                _transport.ConnectAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cn - " + ex.Message);
            }
        }

        private void OnOpen(object source)
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

        private void OnClose(object source)
        {
            try
            {
                // Clear any open subscriptions.
                ClearSubscriptions();

                // Set the status values.
                IsConnected = false;
                Disconnected(this);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":OCl - " + ex.Message);
            }
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _isDisconnecting = true;
                Send(new RequestFrame { Disconnect = new DisconnectRequest() });
                _transport.Close();

                ClearSubscriptions();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":DCn - " + ex.Message);
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
                lock (_subscriptionReferences)
                {
                    // Clear the subscription references.
                    subscriptions = _subscriptionReferences.Values.ToList();
                    _subscriptionReferences.Clear();
                }

                // Handle OnCompleted events for each subscription.
                foreach (IObserver<ByteString> subscription in subscriptions)
                    subscription.OnCompleted();

                lock (_receiptReferences)
                {
                    // Clear the receipt references.
                    receiptables = _receiptReferences.Values.ToList();
                    _receiptReferences.Clear();
                }

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

        /// <summary>
        /// Sends a message to the specified address.
        /// </summary>
        /// <param name="toSend">The Stomp frame to send.</param>
        internal void Send(ProtoStompSend toSend)
        {
            try
            {
                if (toSend.ReceiptID != 0)
                    lock (_receiptReferences)
                        _receiptReferences.Add(toSend.ReceiptID, toSend);

                Send(new RequestFrame { Send = toSend.Request });
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Snd - " + ex.Message);
            }
        }

        /// <summary>
        /// Sends a heartbeat message to the server.
        /// </summary>
        public void SendHeartbeat()
        {
            try
            {
                // Send an empty message to the server.
                Send(new RequestFrame { Heartbeat = new Heartbeat() });
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":SHB - " + ex.Message);
            }
        }

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
                    lock (_subscriptionReferences)
                        _subscriptionReferences[subscription.SubscriptionID] = subscription;

                    if (subscription.ReceiptID != 0)
                        lock (_receiptReferences)
                            _receiptReferences.Add(subscription.ReceiptID, subscription);

                    Send(new RequestFrame { Subscribe = subscription.Request });

                    // Log the subscription action.
                    Trace.TraceInformation(_moduleID + ":Sub - Subscribe: " + subscription.Destination +
                                            " [" + subscription.SubscriptionID.ToString() + "]");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Sub - " + ex.Message);
            }
        }

        /// <summary>
        /// Unsubscribes the specified destination.
        /// </summary>
        internal void Throttle(ProtoStompSubscription subscription, uint rate)
        {
            ThrottleRequest throttle = new ThrottleRequest();
            ProtoStompReceipt receipt = new ProtoStompReceipt();

            try
            {
                if (subscription is object)
                {
                    // Create the unsubscribe message.
                    throttle.SubscriptionId = subscription.SubscriptionID;
                    throttle.ThrottleRate = rate;
                    throttle.ReceiptId = receipt.ReceiptID;

                    // Add to the receiptable requests.
                    lock (_receiptReferences)
                        _receiptReferences.Add(receipt.ReceiptID, receipt);

                    // Send the message.
                    Send(new RequestFrame { Throttle = throttle });

                    // Log the throttle action.
                    Trace.TraceInformation(_moduleID + ":Thr - Throttle: " + subscription.Destination +
                                         " [" + subscription.SubscriptionID.ToString() + "]: " + rate.ToString());
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
            ProtoStompReceipt receipt = new ProtoStompReceipt();
            UnsubscribeRequest unsubscribe = new UnsubscribeRequest();

            try
            {
                if (subscription is object)
                {
                    // Remove from the subscription references.
                    lock (_subscriptionReferences)
                        _subscriptionReferences.Remove(subscription.SubscriptionID);

                    // Create the unsubscribe message.
                    unsubscribe.SubscriptionId = subscription.SubscriptionID;
                    unsubscribe.ReceiptId = receipt.ReceiptID;

                    // Handle the receipt events on the subscription.
                    receipt.Invalidated += ((IReceiptable)subscription).OnInvalidate;
                    receipt.Receipted += ((IObserver<ByteString>)subscription).OnCompleted;

                    // Add to the receiptable requests.
                    lock (_receiptReferences)
                        _receiptReferences.Add(receipt.ReceiptID, receipt);

                    // Send the message.
                    Send(new RequestFrame { Unsubscribe = unsubscribe });

                    // Log the subscription action.
                    Trace.TraceInformation(_moduleID + ":USub - Unsubscribe: " +
                                           subscription.Destination + " [" + subscription.SubscriptionID.ToString() + "]");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":USub - " + ex.Message);
            }
        }

        /// <summary>
        /// Called when [connected] received.
        /// </summary>
        /// <param name="obj">The obj.</param>
        private void OnStompConnected(ResponseFrame frame)
        {
            Task.Run(() =>
            {

                try
                {
                    StompVersion = frame.Connected.Version;
                    IsConnected = true;
                    Connected(this);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(_moduleID + ":OSCn - " + ex.Message);
                }
            });
        }

        /// <summary>
        /// Dispatches the given message to a registered message consumer.
        /// </summary>
        /// <param name="msg">The message to handle.</param>
        private void HandleMessage(object source, byte[] msg)
        {
            ResponseFrame frame;

            try
            {
                frame = ResponseFrame.Parser.ParseFrom(msg);

                if (frame is null)
                    return;

                if (_messageConsumers.TryGetValue(frame.ResponseCase, out Action<ResponseFrame> consumer))
                    consumer(frame);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HMsg - " + ex.Message);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsConnected)
                Disconnect();
        }

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
                                                          throttleRate: (uint) throttleRate)) as TopSymbols;

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
    }
}
