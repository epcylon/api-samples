using QuantGate.API.Proto.Stealth;
using System.Collections.Generic;

namespace BridgeRock.CSharpExample.API.Values
{
    public class SearchResults : ValueBase
    {
        private string _searchTerm;
        private List<SymbolSearchResult> _results = new List<SymbolSearchResult>();

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

        public List<SymbolSearchResult> Results
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
