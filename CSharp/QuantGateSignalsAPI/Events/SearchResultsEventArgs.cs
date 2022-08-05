using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Symbol Search Results values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class SearchResultsEventArgs : SubscriptionEventArgs
    {
        /// <summary>
        /// Search term the results are for.
        /// </summary>
        public string SearchTerm { get; }
        /// <summary>
        /// Search results.
        /// </summary>
        public IReadOnlyList<SearchResult> Results { get; }        

        /// <summary>
        /// Creates a new SearchResultsEventArgs instance.
        /// </summary>
        /// <param name="searchTerm">Search term the results are for.</param>
        /// <param name="results">Search results.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal SearchResultsEventArgs(string searchTerm, List<SearchResult> results, 
                                        SubscriptionError error = null) : base(error)
        {
            SearchTerm = searchTerm;
            Results = results;
        }
    }
}
