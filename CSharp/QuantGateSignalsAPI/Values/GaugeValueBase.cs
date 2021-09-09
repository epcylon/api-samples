using System;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Base class for gauge values.
    /// </summary>
    public abstract class GaugeValueBase : EventArgs
    {
        /// <summary>
        /// The symbol being subscribed to for this gauge.
        /// </summary>
        public string Symbol { get; internal set; }

        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime TimeStamp { get; internal set; }

        /// <summary>
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </summary>
        public bool IsDirty { get; internal set; }
    }
}
