using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Values
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
        /// Creates a new TopSymbolsEventArgs instance.
        /// </summary>
        /// <param name="symbols">Top symbol results.</param>
        public TopSymbolsEventArgs(List<TopSymbol> symbols)
        {
            Symbols = symbols;
        }
    }
}
