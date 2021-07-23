﻿using Epcylon.Common.Net.ProtoStomp.Proto;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;

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
        private readonly WebSocket _transport;

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
        public string Host { get; }

        /// <summary>
        /// The port of the server to connect to.
        /// </summary>
        public int Port { get; }

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
        public ProtoStompClient(string host, int port = int.MinValue)
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
        /// Gets a value indicating whether this <see cref = "ProtoStompClient" /> is connected.
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
        public void Send(ProtoStompSend toSend)
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
    }
}
