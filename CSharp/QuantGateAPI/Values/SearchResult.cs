using QuantGate.API.Proto.Stealth;

namespace QuantGate.API.Values
{
    public class SearchResult
    {
        public object Symbol { get; internal set; }
        public string Underlying { get; internal set; }
        public string Currency { get; internal set; }
        public string DisplayName { get; internal set; }
        public string Exchange { get; internal set; }
        internal InstrumentType InstrumentType { get; set; }
    }
}
