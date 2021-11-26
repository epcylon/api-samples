using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Top Symbol values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class TopSymbolsEventArgs : EventArgs
    {
        /// <summary>
        /// Top symbol results.
        /// </summary>
        public IReadOnlyList<TopSymbol> Symbols { get; }
        /// <summary>
        /// Holds error information, if a subscription error occured.
        /// </summary>
        public SubscriptionError Error { get; }

        /// <summary>
        /// Creates a new TopSymbolsEventArgs instance.
        /// </summary>
        /// <param name="symbols">Top symbol results.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        public TopSymbolsEventArgs(List<TopSymbol> symbols, SubscriptionError error = null)
        {
            Symbols = symbols;
            Error = error;
        }
    }
}
