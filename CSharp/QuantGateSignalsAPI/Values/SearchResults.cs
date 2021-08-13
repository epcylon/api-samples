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
        private string _searchTerm;
        /// <summary>
        /// Search results.
        /// </summary>
        private List<SearchResult> _results = new List<SearchResult>();

        /// <summary>
        /// Search term the results are for.
        /// </summary>
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Search results.
        /// </summary>
        public List<SearchResult> Results
        {
            get => _results;
            set
            {
                bool changed = false;

                if (_results.Count != value.Count)
                {
                    changed = true;
                }
                else
                {
                    for (int index = 0; index < _results.Count; index++)
                    {
                        if (_results[index].Symbol != value[index].Symbol)
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (changed)
                {
                    _results.Clear();
                    _results.AddRange(value);
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
