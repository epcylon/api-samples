namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds the details of a subscription error from the server.
    /// </summary>
    /// <remarks>
    /// Creates a new SubscriptionError instance.
    /// </remarks>
    /// <param name="message">A message that describes the error that occurred.</param>
    /// <param name="details">Detailed description about the error (if supplied).</param>
    public class SubscriptionError(string message, string details)
    {
        /// <summary>
        /// A message that describes the error that occurred.
        /// </summary>
        public string Message { get; } = message;

        /// <summary>
        /// Detailed description about the error (if supplied). 
        /// </summary>
        public string Details { get; } = details;
    }
}
