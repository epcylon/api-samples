using System;

namespace QuantGate.API.Values
{
    /// <summary>
    /// The details of an individual symbol in response to a top symbols request.
    /// </summary>
    public class TopSymbol : SearchResult
    {
        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime Timestamp { get; internal set; }
        /// <summary>
        /// Entry progress value.
        /// </summary>
        public double EntryProgress { get; internal set; }
        /// <summary>
        /// Signal tied to the Perception level.
        /// </summary>
        public GaugeSignal PerceptionSignal { get; internal set; }
        /// <summary>
        /// Signal tied to the Commitment level.
        /// </summary>
        public GaugeSignal CommitmentSignal { get; internal set; }
        /// <summary>
        /// Signal tied to the Equilibrium level.
        /// </summary>
        public GaugeSignal EquilibriumSignal { get; internal set; }
        /// <summary>
        /// Signal tied to the Sentiment level.
        /// </summary>
        public GaugeSignal SentimentSignal { get; internal set; }
        /// <summary>
        /// Entry signal for the default strategy.
        /// </summary>
        public StrategySignal Signal { get; internal set; }
    }
}
