using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Futures List response details.
    /// </summary>
    public class FuturesListEventArgs : EventArgs
    {
        /// <summary>
        /// The underlying for which futures were requested.
        /// </summary>
        public string Underlying { get; }
        /// <summary>
        /// The currency of the futures that were requested.
        /// </summary>
        public string Currency { get; }
        /// <summary>
        /// The list of futures returned.
        /// </summary>
        public IReadOnlyList<InstrumentBase> Futures { get; }
        /// <summary>
        /// Holds error information, if a subscription error occured.
        /// </summary>
        public SubscriptionError Error { get; }

        /// <summary>
        /// Creates a new FuturesListEventArgs instance.
        /// </summary>
        /// <param name="underlying">The underlying for which futures were requested.</param>
        /// <param name="currency">The currency of the futures that were requested.</param>
        /// <param name="futures">The list of futures returned.</param>
        internal FuturesListEventArgs(string underlying, string currency, List<InstrumentBase> futures)
        {
            Underlying = underlying;
            Currency = currency;
            Futures = futures;
            Error = null;
        }

        /// <summary>
        /// Creates a new FuturesListEventArgs instance.
        /// </summary>
        /// <param name="underlying">The underlying for which futures were requested.</param>
        /// <param name="currency">The currency of the futures that were requested.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal FuturesListEventArgs(string underlying, string currency, SubscriptionError error)
        {
            Underlying = underlying;
            Currency = currency;
            Futures = new List<InstrumentBase>();
            Error = error;
        }
    }
}
