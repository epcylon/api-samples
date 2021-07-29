using System;
using System.Collections.Generic;

namespace QuantGate.API.Values
{
    public class SearchUpdateEventArgs : EventArgs
    {
        public string SearchTerm { get; }
        public IReadOnlyList<SearchResult> Results { get; }

        internal SearchUpdateEventArgs(string term, List<SearchResult> results)
        {
            SearchTerm = term;
            Results = results;
        }
    }
}
