using System;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Strategy values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class StrategyValues : ValueBase
    {
        /// <summary>
        /// Symbol to get the Strategy update data for.
        /// </summary>
        public string Symbol { get; internal set; }
        /// <summary>
        /// Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0,  Crb.8.4.
        /// </summary>
        public string StrategyID { get; internal set; }

        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime TimeStamp { get; internal set; }

        /// <summary>
        /// Entry progress value.
        /// </summary>
        public double EntryProgress { get; internal set; }

        /// <summary>
        /// Exit progress value.
        /// </summary>
        public double ExitProgress { get; internal set; }

        /// <summary>
        /// Entry signal for the strategy.
        /// </summary>
        public StrategySignal Signal { get; internal set; }

        /// <summary>
        /// Perception level. 0 represents an unset value.
        /// </summary>
        public double? PerceptionLevel { get; internal set; }

        /// <summary>
        /// Signal tied to the perception level.
        /// </summary>
        public GaugeSignal PerceptionSignal { get; internal set; }

        /// <summary>
        /// Commitment level. 0 represents an unset value.
        /// </summary>
        public double? CommitmentLevel { get; internal set; }

        /// <summary>
        /// Signal tied to the commitment level.
        /// </summary>
        public GaugeSignal CommitmentSignal { get; internal set; }

        /// <summary>
        /// Equilibrium level. 0 represents an unset value.
        /// </summary>
        public double? EquilibriumLevel { get; internal set; }

        /// <summary>
        /// Signal tied to the equilibrium level.
        /// </summary>
        public GaugeSignal EquilibriumSignal { get; internal set; }

        /// <summary>
        /// Sentiment level. 0 represents an unset value.
        /// </summary>
        public double? SentimentLevel { get; internal set; }

        /// <summary>
        /// Signal tied to the 50t sentiment indication.
        /// </summary>
        public GaugeSignal SentimentSignal { get; internal set; }
    }
}
