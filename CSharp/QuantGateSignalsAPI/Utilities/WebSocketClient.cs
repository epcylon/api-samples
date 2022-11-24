using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace QuantGate.API.Signals.Utilities
{
    internal class WebSocketClient
    {
        #region Constants

        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "WsCB";

        /// <summary>
        /// Maximum number of concurrent messages (queue should not exceed this).
        /// </summary>
        private readonly int _maxConcurrent;

        /// <summary>
        /// The type of message to send/receive.
        /// </summary>
        private readonly WebSocketMessageType _messageType;
        /// <summary>
        /// The type of message to not accept.
        /// </summary>
        private readonly WebSocketMessageType _invalidType;

        #endregion

        #region Public Events

        /// <summary>
        /// Called whenever the transport object connects.
        /// </summary>
        public event EventHandler OnOpen = delegate { };
        /// <summary>
        /// Called whenever the transport object disconnects.
        /// </summary>
        public event EventHandler OnClose = delegate { };
        /// <summary>
        /// Called whenever a new message is received from the server.
        /// </summary>        
        public event EventHandler<byte[]> OnMessage = delegate { };

        #endregion

        #region Private Properties

        /// <summary>
        /// Cancellation token for the send loop.
        /// </summary>
        private CancellationTokenSource _sendLoopTokenSource;
        /// <summary>
        /// Cancellation token for the receive loop.
        /// </summary>
        private CancellationTokenSource _receiveLoopTokenSource;

        /// <summary>
        /// Blocking message queue used in the main thread to process new actions within the thread.
        /// </summary>
        private BlockingCollection<byte[]> _messageQueue;

        /// <summary>
        /// Synchronization context to send events through (if supplied).
        /// </summary>
        private readonly SynchronizationContext _context;

        /// <summary>
        /// The WebSocket this client is connected to.
        /// </summary>
        private WebSocket _socket = default;

        /// <summary>
        /// Lock object used for access to socket.
        /// </summary>
        private readonly object _lock = new object();

        #endregion

        #region Public Properties        

        /// <summary>
        /// Returns the current WebSocket state.
        /// </summary>
        public WebSocketState State 
        { 
            get
            {
                lock (_lock)
                {
                    if (_socket is null)
                        return WebSocketState.Closed;
                    else
                        return _socket.State;
                }
            }
        }
        /// <summary>
        /// The URI that the client is connecting/connected to.
        /// </summary>
        public Uri Uri { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new WebSocketClientBase instance.
        /// </summary>
        /// <param name="uri">The URI that the client is connecting to</param>
        /// <param name="context">Synchronization context to send events through (if supplied).</param>
        /// <param name="maxConcurrent">Maximum number of concurrent messages (queue should not exceed this).</param>
        public WebSocketClient(Uri uri, SynchronizationContext context = null, int maxConcurrent = 10000)
        {
            Uri = uri;            
            _maxConcurrent = maxConcurrent;
            _context = context;

            _messageType = WebSocketMessageType.Binary;            
            _invalidType = WebSocketMessageType.Text;
        }

        /// <summary>
        /// Connects the WebSocket client.
        /// </summary>
        public void Connect()
        {
            lock (_lock)
            {
                // Only allow connection once.
                if (_socket is object)
                    throw new InvalidOperationException("Cannot connect WebSocket twice.");

                // Set connection flag.
                _socket = new ClientWebSocket();
            }

            // Do the actual connection.
            ConnectAsync();
        }

        /// <summary>
        /// Connects to a websocket server asynchronously and handles the result.
        /// </summary>
        private async void ConnectAsync()
        {
            try
            {
                // Initialize the send queue.
                InitializeSend();

                // Connect to the client.
                await ((ClientWebSocket)_socket).
                    ConnectAsync(Uri, CancellationToken.None).ConfigureAwait(false);

                if (_socket.State == WebSocketState.Open)
                {
                    // If opened correctly, inform and handle messages.
                    SendOnOpen();
                    InitializeReceive();
                }
                else
                {
                    // If not opened correctly, inform of closure.
                    SendOnClose();
                }
            }
            catch (Exception ex)
            {
                // If an exception occurred, assume closed.
                Trace.TraceError(_moduleID + ":CnAs - " + ex.Message);
                SendOnClose();
            }
        }

        /// Used to close the client and disconnect.
        /// </summary>
        /// <param name="status">The status to return.</param>
        /// <param name="message">The close message to include.</param>
        public void Close(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
                          string message = "Normal closure.")
        {
            if (status == WebSocketCloseStatus.NormalClosure)
            {
                // Normal closure, add the closure as the last message to the queue.
                _messageQueue.Add(null);
                _messageQueue.CompleteAdding();
            }
            else
            {
                // Close asynchronously, and ignore the result.
                _ = CloseAsync(status, message).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Used to close the client and disconnect.
        /// </summary>
        /// <param name="status">The status to return.</param>
        /// <param name="message">The close message to include.</param>
        private async Task CloseAsync(WebSocketCloseStatus status, string message)
        {
            try
            {
                // Cancel the send loop.
                _sendLoopTokenSource.Cancel();

                // If not open, no need to close anything.
                if (_socket.State != WebSocketState.Open &&
                    _socket.State != WebSocketState.Connecting) return;

                // Close the socket first, because ReceiveAsync leaves an invalid
                // socket (state = aborted) when the token is cancelled
                var timeout = new CancellationTokenSource(10000);
                try
                {
                    // after this, the socket state which change to CloseSent
                    // Close as a normal closure.
                    await _socket.CloseOutputAsync(status, message, timeout.Token).ConfigureAwait(false);
                    // now we wait for the server response, which will close the socket
                    while (_socket.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested) ;
                }
                catch (OperationCanceledException)
                {
                    // normal upon task/token cancellation, disregard
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":Cls - " + ex.Message);
            }
            finally
            {
                // Whether we closed the socket or timed out, we cancel the token causing
                // RecieveAsync to abort the socket.
                _receiveLoopTokenSource.Cancel();
            }
        }

        #endregion

        #region Receive Handling

        /// <summary>
        /// Initializes the receive thread.
        /// </summary>
        protected void InitializeReceive()
        {
            // Start the receive thread.
            _ = Task.Factory.StartNew(() => HandleMessages().ConfigureAwait(false),
                                      TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Handles the messages being sent to the Websocket connection.
        /// </summary>
        private async Task HandleMessages()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult receiveResult;
            CancellationToken token = _receiveLoopTokenSource.Token;
            MemoryStream stream = new MemoryStream();

            try
            {
                while (_socket.State != WebSocketState.Closed && !token.IsCancellationRequested)
                {
                    // While the socket has not closed, receive the next chunk.
                    receiveResult = await _socket.ReceiveAsync(buffer, token);

                    if (receiveResult.MessageType == _invalidType)
                    {
                        // If the message is text, this is unsupported.
                        Close(WebSocketCloseStatus.InvalidMessageType, "Cannot accept this message type.");
                    }
                    else if (!token.IsCancellationRequested)
                    {
                        // If the token is cancelled while ReceiveAsync is blocking,
                        // the socket state changes to aborted and it can't be used. 
                        // the server is notifying us that the connection will close; send acknowledgement
                        if (_socket.State == WebSocketState.CloseReceived &&
                            receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            // Acknowledgeing close received from the server.
                            _sendLoopTokenSource.Cancel();
                            await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                                                          "Acknowledge Close frame", CancellationToken.None);
                        }

                        if (_socket.State == WebSocketState.Open &&
                            receiveResult.MessageType != WebSocketMessageType.Close)
                        {
                            // If still open, write the data from the buffer into the stream.
                            stream.Write(buffer.Array!, buffer.Offset, receiveResult.Count);

                            if (receiveResult.EndOfMessage)
                            {
                                // If this is the end of the message, let listeners handle the message.
                                SendOnMessage(stream.ToArray());
                                // Dispose of the stream and create a new one.
                                stream.Dispose();
                                stream = new MemoryStream();
                            }
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Normal upon task/token cancellation, disregard
            }
            catch (WebSocketException wex)
            {
                // If not premature closure, log the exception.
                if (wex.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                    Trace.TraceError(_moduleID + ":HMsgs - " + wex.Message);
            }
            catch (Exception ex)
            {
                // Log any unexpected exceptions.
                Trace.TraceError(_moduleID + ":HMsgs - " + ex.Message);
            }
            finally
            {
                _sendLoopTokenSource.Cancel();
                stream.Dispose();
                _socket.Dispose();

                // When done with everything, inform of closure.
                SendOnClose();
            }
        }

        #endregion

        #region Send Handling

        /// <summary>
        /// Initializes the send thread.
        /// </summary>
        protected void InitializeSend()
        {
            // Start the send thread.
            _receiveLoopTokenSource = new CancellationTokenSource();
            _messageQueue = new BlockingCollection<byte[]>();
            _ = Task.Factory.StartNew(() => HandleSends().ConfigureAwait(false),
                                      TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// This is the main send loop.
        /// </summary>
        private async Task HandleSends()
        {
            _sendLoopTokenSource = new CancellationTokenSource();
            CancellationToken token = _sendLoopTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested && !_messageQueue.IsCompleted)
                {
                    try
                    {
                        // Go through each action in the blocking collection while not cancelled.
                        byte[] message = _messageQueue.Take(token);

                        // Send if this is a message, otherwise close if it is a closure request.
                        if (message is object)
                            await SendMessageAsync(message).ConfigureAwait(false);
                        else
                            await CloseAsync(WebSocketCloseStatus.NormalClosure,
                                             "Normal closure").ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Normal upon task/token cancellation, disregard
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HS-2 - " + ex.Message);
                Close(WebSocketCloseStatus.InternalServerError, "Error in send loop.");
            }
        }

        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="toSend">The message to send.</param>
        private async Task SendMessageAsync(byte[] message)
        {
            try
            {
                // Send the data to the socket, and wait for the result.
                if (_socket.State == WebSocketState.Open)
                    await _socket.SendAsync(new ArraySegment<byte>(message), _messageType,
                                            true, new CancellationTokenSource(30000).Token);
            }
            catch (OperationCanceledException)
            {
                // Log an error message.
                Trace.TraceError(_moduleID + ":HS-1 - Client disconnected due a timed out send operation");                
                // Disconnect (overloaded).
                Close(WebSocketCloseStatus.EndpointUnavailable, "Send timeout.");
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HS-2 - " + ex.Message);
                Close(WebSocketCloseStatus.InternalServerError, "Error during send.");
            }
        }

        /// <summary>
        /// Used to send a message to the client.
        /// </summary>
        /// <param name="payload">The message to send to the client.</param>
        /// <param name="sendAsync">Send asynchronously?</param>
        public void Send(byte[] payload)
        {
            if (_messageQueue.Count > _maxConcurrent)
            {
                // If the queue has too many items in it, close the connection.
                Close(WebSocketCloseStatus.InternalServerError,
                      "Too many messages unsent - resetting connection.");
                return;
            }

            // Just use the async method here - using wait could cause deadlocks.
            _messageQueue.Add(payload);
        }

        #endregion

        #region Event Sending

        /// <summary>
        /// Sends the OnOpen event, using the synchronization context if necessary.
        /// </summary>
        protected void SendOnOpen()
        {
            if (_context is null || _context == SynchronizationContext.Current)
                OnOpen(this, EventArgs.Empty);
            else
                _context.Post(new SendOrPostCallback(o => OnOpen(this, EventArgs.Empty)), null);
        }

        /// <summary>
        /// Sends the OnMessage event, using the synchronization context if necessary.
        /// </summary>
        /// <param name="message">The message to send.</param>
        protected void SendOnMessage(byte[] message)
        {
            if (_context is null || _context == SynchronizationContext.Current)
                OnMessage(this, message);
            else
                _context.Post(new SendOrPostCallback(o => OnMessage(this, message)), null);
        }

        /// <summary>
        /// Sends the OnClose event, using the synchronization context if necessary.
        /// </summary>
        protected void SendOnClose()
        {
            lock (_lock)
                _socket = default;

            if (_context is null || _context == SynchronizationContext.Current)
                OnClose(this, EventArgs.Empty);
            else
                _context.Post(new SendOrPostCallback(o => OnClose(this, EventArgs.Empty)), null);
        }

        #endregion
    }
}
