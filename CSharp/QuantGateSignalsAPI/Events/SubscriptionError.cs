namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds the details of a subscription error from the server.
    /// </summary>
    public class SubscriptionError
    {
        /// <summary>
        /// A message that describes the error that occured.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Detailed description about the error (if supplied). 
        /// </summary>
        public string Details { get; }

        /// <summary>
        /// Creates a new SubscriptionError instance.
        /// </summary>
        /// <param name="message">A message that describes the error that occured.</param>
        /// <param name="details">Detailed description about the error (if supplied).</param>
        public SubscriptionError(string message, string details)
        {
            Message = message;
            Details = details;
        }
    }
}
