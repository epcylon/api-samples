using BridgeRock.CSharpExample.WebSockets;
using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BridgeRock.CSharpExample.ProtoStomp
{
    /// <summary>
    /// Ultra simple protobuf STOMP client with command buffering support
    /// </summary>
    public class ProtoStompClient : IDisposable
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
        private readonly ITransport<byte[], byte[]> _transport;

        /// <summary>
        /// The Stomp version the client is connected to.
        /// </summary>
        public string StompVersion { get; private set; }

        public event Action<ProtoStompClient> Connected = delegate { };
        public event Action<ProtoStompClient> Disconnected = delegate { };
        public event Action<ProtoStompClient> OnHeartbeat = delegate { };
        public event Action<ProtoStompClient, string> Error = delegate { };

        /// <summary>
        /// The host address of the server to connect to.
        /// </summary>
        public string Host => _transport.Host;

        /// <summary>
        /// The port of the server to connect to.
        /// </summary>
        public int Port => _transport.Port;

        public string Username { get; private set; }
        public string Password { get; private set; }
        private ByteString ConnectBody { get; }
        
        /// <summary>
        /// Is the client disconnecting?
        /// </summary>
        private bool _isDisconnecting = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="StompClient" /> class.
        /// </summary>
        /// <param name = "transport">The transport channel.</param>
        /// <param name="username">The username to connect with.</param>
        /// <param name="password">The password to connect with.</param>
        /// <param name="connectBody">Any extra information to include in the connect message.</param>
        public ProtoStompClient(ITransport<byte[], byte[]> transport, string username = null,
                                string password = null, ByteString connectBody = null)
        {
            _transport = transport;

            _transport.OnOpen += OnOpen;
            _transport.OnClose += OnClose;
            _transport.OnMessage += HandleMessage;
            _transport.OnError += HandleError;

            Username = username;
            Password = password;
            ConnectBody = connectBody;

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

        private void HandleSubscriptionError(ResponseFrame frame)
        {
            ProtoStompSubscription subscription;

            try
            {
                // If the subscription id exists, try to get the subscription.
                lock (_subscriptionReferences)
                    _subscriptionReferences.TryGetValue(
                        frame.SubscriptionError.SubscriptionId, out subscription);

                if (subscription != null)
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
                
                if (subscription != null)
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

                if (receiptable != null)
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
        /// Gets a value indicating whether this <see cref = "ProtoStompClient" /> is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Connects to the server on the specified address.
        /// </summary>
        /// <param name="username">The username to connect with.</param>
        /// <param name="password">The password to connect with.</param>
        public void Connect(string username = null, string password = null)
        {
            try
            {
                if (username is object)
                    Username = username;

                if (password is object)
                    Password = password;

                _isDisconnecting = false;
                _transport.Connect();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cn - " + ex.Message);
            }
        }

        private void HandleError(ITransport<byte[], byte[]> source, string message)
        {
            Trace.TraceError(_moduleID + ":HE - Stomp transport error: " + message);
        }

        private void OnOpen(ITransport<byte[], byte[]> source)
        {
            ConnectRequest connect;

            try
            {
                connect = new ConnectRequest { AcceptVersion = "1.0" };

                if (Username is object)
                    connect.Login = Username;
                if (Password is object)
                    connect.Passcode = Password;
                if (ConnectBody is object)
                    connect.Body = ConnectBody;
                                
                _transport.Send(new RequestFrame { Connect = connect }.ToByteArray());
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":OO - " + ex.Message);
            }
        }

        private void OnClose(ITransport<byte[], byte[]> source)
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
                _transport.Send(new RequestFrame { Disconnect = new DisconnectRequest() }.ToByteArray());
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
        public void Send(ProtoStompSend toSend)
        {            
            try
            {
                if (toSend.ReceiptID != 0)
                    lock (_receiptReferences)
                        _receiptReferences.Add(toSend.ReceiptID, toSend);

                _transport.Send(new RequestFrame { Send = toSend.Request }.ToByteArray());
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
                _transport.Send(new RequestFrame { Heartbeat = new Heartbeat() }.ToByteArray());
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
                if (subscription != null)
                {
                    lock (_subscriptionReferences)
                        _subscriptionReferences[subscription.SubscriptionID] = subscription;

                    if (subscription.ReceiptID != 0)
                        lock (_receiptReferences)
                            _receiptReferences.Add(subscription.ReceiptID, subscription);

                    _transport.Send(new RequestFrame { Subscribe = subscription.Request }.ToByteArray());

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
                if (subscription != null)
                {
                    // Create the unsubscribe message.
                    throttle.SubscriptionId = subscription.SubscriptionID;
                    throttle.ThrottleRate = rate;
                    throttle.ReceiptId = receipt.ReceiptID;

                    // Add to the receiptable requests.
                    lock (_receiptReferences)
                        _receiptReferences.Add(receipt.ReceiptID, receipt);

                    // Send the message.
                    _transport.Send(new RequestFrame { Throttle = throttle }.ToByteArray());

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
                if (subscription != null)
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
                    _transport.Send(new RequestFrame { Unsubscribe = unsubscribe }.ToByteArray());

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
        private void HandleMessage(ITransport<byte[], byte[]> source, byte[] msg)
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
    }
}
