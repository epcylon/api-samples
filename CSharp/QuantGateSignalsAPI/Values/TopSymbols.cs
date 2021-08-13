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
        private List<TopSymbol> _symbols = new List<TopSymbol>();

        /// <summary>
        /// Top symbol results.
        /// </summary>
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
