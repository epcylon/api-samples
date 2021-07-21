namespace BridgeRock.CSharpExample.WebSockets
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">The format to use to send messages to the server.</typeparam>
    /// <typeparam name="R">The format to received messages from the server.</typeparam>
    /// <param name="source">The source of the message.</param>
    public delegate void TransportDelegate<S, R>(ITransport<S, R> source);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">The format to use to send messages to the server.</typeparam>
    /// <typeparam name="R">The format to received messages from the server.</typeparam>
    /// <param name="source">The source of the message.</param>
    /// <param name="message">The message received.</param>
    public delegate void TransportMessageDelegate<S, R>(ITransport<S, R> source, R message);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">The format to use to send messages to the server.</typeparam>
    /// <typeparam name="R">The format to received messages from the server.</typeparam>
    /// <param name="source">The source of the message.</param>
    /// <param name="message">The error message received.</param>
    public delegate void TransportErrorDelegate<S, R>(ITransport<S, R> source, string message);

    /// <summary>
    /// Represents an interface to a transport layer object that sends and receives
    /// messages in formats S and R respectively.
    /// </summary>
    /// <typeparam name="S">The format to use to send messages to the server.</typeparam>
    /// <typeparam name="R">The format to received messages from the server.</typeparam>
    public interface ITransport<S, R>
    {
        /// <summary>
        /// Called whenever the transport object connects.
        /// </summary>
        event TransportDelegate<S, R> OnOpen;
        /// <summary>
        /// Called whenever the transport object disconnects.
        /// </summary>
        event TransportDelegate<S, R> OnClose;
        /// <summary>
        /// Called whenever a new message is received from the server.
        /// </summary>        
        event TransportMessageDelegate<S, R> OnMessage;
        /// <summary>
        /// Called whenever an error is received from the server.
        /// </summary>
        event TransportErrorDelegate<S, R> OnError;

        /// <summary>
        /// Returns the host location connected to.
        /// </summary>
        string Host { get; }
        /// <summary>
        /// Returns the port of the server connected to.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Connects to the transport object.
        /// </summary>
        /// <remarks>
        /// All action events must be registered before calling connect.
        /// </remarks>
        void Connect();
        /// <summary>
        /// Used to close the transport object.
        /// </summary>
        void Close();
        /// <summary>
        /// Sends a message to the transport object.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(S message);
    }
}