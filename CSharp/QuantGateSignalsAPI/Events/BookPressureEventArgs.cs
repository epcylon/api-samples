using System;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Book Pressure values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class BookPressureEventArgs : GaugeArgsBase
    {
        /// <summary>
        /// The gauge value at the last update.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Creates a new BookPressureEventArgs instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="value">The gauge value at the last update.</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal BookPressureEventArgs(string symbol, DateTime timestamp, double value, bool isDirty, 
                                       object reference = null, SubscriptionError error = null) :
            base(symbol, timestamp, isDirty, reference, error)
        {
            Value = value;
        }
    }
}
