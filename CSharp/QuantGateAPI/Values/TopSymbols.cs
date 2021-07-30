using System.Collections.Generic;

namespace QuantGate.API.Values
{
    public class TopSymbols : ValueBase
    {
        private List<TopSymbol> _symbols = new List<TopSymbol>();

        public List<TopSymbol> Symbols
        {
            get => _symbols;
            set
            {
                bool changed = false;

                if (_symbols.Count != value.Count)
                {
                    changed = true;
                }
                else
                {
                    for (int index = 0; index < _symbols.Count; index++)
                    {
                        if (_symbols[index].Symbol != value[index].Symbol ||
                            _symbols[index].Timestamp.Ticks != value[index].Timestamp.Ticks)
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (changed)
                {
                    _symbols.Clear();
                    _symbols.AddRange(value);
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
