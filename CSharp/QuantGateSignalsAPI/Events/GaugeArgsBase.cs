﻿using System;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Base class for gauge values.
    /// </summary>
    public abstract class GaugeArgsBase : EventArgs
    {
        /// <summary>
        /// The symbol being subscribed to for this gauge.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </summary>
        public bool IsDirty { get; }

        /// <summary>
        /// Holds error information, if a subscription error occured.
        /// </summary>
        public SubscriptionError Error { get; }

        /// <summary>
        /// Creates a new GaugeArgsBase instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Subscription error information, if an error occured.</param>
        internal GaugeArgsBase(string symbol, DateTime timestamp, bool isDirty, SubscriptionError error)
        {
            Symbol = symbol;
            TimeStamp = timestamp;
            IsDirty = isDirty;
            Error = error;
        }
    }
}
