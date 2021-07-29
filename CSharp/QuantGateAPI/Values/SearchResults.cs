using System.Collections.Generic;

namespace QuantGate.API.Values
{
    internal class SearchResults : ValueBase
    {
        private string _searchTerm;
        private List<SearchResult> _results = new List<SearchResult>();

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
