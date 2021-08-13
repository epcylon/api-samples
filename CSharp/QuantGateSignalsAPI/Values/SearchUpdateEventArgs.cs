using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Arguments to be returned in a Search Update event.
    /// </summary>
    public class SearchUpdateEventArgs : EventArgs
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
        /// Creates a new SearchUpdateEventArgs instance.
        /// </summary>
        /// <param name="term">Search term the results are for.</param>
        /// <param name="results">Search results.</param>
        internal SearchUpdateEventArgs(string term, List<SearchResult> results)
        {
            SearchTerm = term;
            Results = results;
        }
    }
}
