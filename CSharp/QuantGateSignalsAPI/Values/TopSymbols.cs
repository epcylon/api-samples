using System.Collections.Generic;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Top Symbol values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class TopSymbols : ValueBase
    {
        /// <summary>
        /// Top symbol results.
        /// </summary>
        public List<TopSymbol> Symbols { get; internal set; }
    }
}
