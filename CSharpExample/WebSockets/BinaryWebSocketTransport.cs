using System;
using System.Diagnostics;
using System.Net;
using WebSocketSharp;

namespace BridgeRock.CSharpExample.WebSockets
{
    /// <summary>
    /// Websocket binary wrapper.
    /// </summary>
    public class BinaryWebSocketTransport : ITransport<byte[], byte[]>
    {
        #region Constants

        /// <summary>
        /// Class-level identifier.
        /// </summary>
        private const string _moduleID = "BWST";

        #endregion

        #region Public Events

        /// <summary>
        /// Called whenever the transport object connects.
        /// </summary>
        public event TransportDelegate<byte[], byte[]> OnOpen = delegate { };
        /// <summary>
        /// Called whenever the transport object disconnects.
        /// </summary>
        public event TransportDelegate<byte[], byte[]> OnClose = delegate { };
        /// <summary>
        /// Called whenever a new message is received from the server.
        /// </summary>        
        public event TransportMessageDelegate<byte[], byte[]> OnMessage = delegate { };
        /// <summary>
        /// Called whenever an error is received from the server.
        /// </summary>
        public event TransportErrorDelegate<byte[], byte[]> OnError = delegate { };
        

        #endregion

        #region Class Members

        /// <summary>
        /// The host address of the server to connect to.
        /// </summary>
        public string Host { get; }
        /// <summary>
        /// The port of the server to connect to.
        /// </summary>
        public int Port { get; }        

        /// <summary>
        /// Internal WebSocket instance.
        /// </summary>
        private WebSocket _webSocket;

        /// <summary>
        /// Allow untrusted certificates?
        /// </summary>
        private readonly bool _allowUntrusted;

        #endregion

        #region Initialization / Finalization

        /// <summary>
        /// Creates a new WebSocketTransport instance.
        /// </summary>
        /// <param name="host">The web address to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="allowUntrusted">Allow untrusted certificates?</param>
        public BinaryWebSocketTransport(string host, int port = int.MinValue, bool allowUntrusted = false)
        {
            if (allowUntrusted)
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

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
            _allowUntrusted = allowUntrusted;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects to the transport object.
        /// </summary>
        /// <remarks>
        /// All action events must be registered before calling connect.
        /// </remarks>
        void ITransport<byte[], byte[]>.Connect()
        {
            try
            {
                // Create the new websocket.
                _webSocket = new WebSocket(Host + ':' + Port + "/");

                // Set up the event handling.
                _webSocket.OnOpen += (o, e) => OnOpen(this);
                _webSocket.OnClose += (o, e) => OnClose(this);
                _webSocket.OnMessage += (o, e) => OnMessage(this, e.RawData);
                _webSocket.OnError += (o, e) =>
                {
                    // Handle the error message.
                    OnError(this, e.Message + (e.Exception?.Message ?? string.Empty));

                    // Make sure it closes properly.                        
                    if ((_webSocket != null) && (_webSocket.ReadyState == WebSocketState.Closed))
                        ((ITransport<string, string>)this).Close();
                };

                // Connect to the websocket.
                _webSocket.ConnectAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cn - " + ex.Message);
                OnError(this, ex.Message);
            }
        }

        /// <summary>
        /// Sends a message to the transport object.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void ITransport<byte[], byte[]>.Send(byte[] message)
        {
            try
            {
                _webSocket?.SendAsync(message, OnSent);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Snd - " + ex.Message);
            }
        }

        /// <summary>
        /// Handle asynchronous send completions.
        /// </summary>
        /// <param name="success">Was the send successful?</param>
        private void OnSent(bool success)
        {
            try
            {
                // Decrement sending and if no longer throttled, get the parent.                
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + "OS - " + ex.Message);
            }
        }

        /// <summary>
        /// Used to close the transport object.
        /// </summary>
        void ITransport<byte[], byte[]>.Close()
        {
            try
            {
                if (_webSocket != null)
                {
                    if (_webSocket.ReadyState != WebSocketState.Closed)
                        _webSocket.CloseAsync();
                    else
                        OnClose(this);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cls - " + ex.Message);
                OnClose(this);
            }
            finally
            {
                _webSocket = null;
            }
        }

        #endregion
    }
}
