using System;

namespace QuantGate.API.Values
{
    public class TopSymbol : SearchResult
    {
        public DateTime Timestamp { get; internal set; }
        public double EntryProgress { get; internal set; }
        public GaugeSignal PerceptionSignal { get; internal set; }
        public GaugeSignal CommitmentSignal { get; internal set; }
        public GaugeSignal EquilibriumSignal { get; internal set; }
        public GaugeSignal SentimentSignal { get; internal set; }
        public StrategySignal Signal { get; internal set; }
    }
}
