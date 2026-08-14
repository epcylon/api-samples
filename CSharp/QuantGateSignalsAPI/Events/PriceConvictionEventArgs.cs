namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds PriceConviction values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class PriceConvictionEventArgs : GaugeEventArgs
    {
        /// <summary>
        /// The raw, direction-agnostic, volatility-normalized price-conviction value at the
        /// last update (unbounded -- not compressed/bounded like most other gauge values).
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Creates a new PriceConvictionEventArgs instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="value">The gauge value at the last update.</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal PriceConvictionEventArgs(string symbol, DataStream stream, DateTime timestamp, double value,
                                          bool isDirty, SubscriptionError error = null) :
            base(symbol, stream, timestamp, isDirty, error)
        {
            Value = value;
        }
    }
}
