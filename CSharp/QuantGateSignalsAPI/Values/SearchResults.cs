using QuantGate.API.Signals.Events;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Symbol Search Results values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    internal class SearchResults : ValueBase
    {
        /// <summary>
        /// Search term the results are for.
        /// </summary>
        public string SearchTerm { get; internal set; }

        /// <summary>
        /// Search results.
        /// </summary>
        public List<SearchResult> Results { get; internal set; }
    }
}
