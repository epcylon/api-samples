using System;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Strategy values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class StrategyEventArgs : EventArgs
    {
        /// <summary>
        /// Symbol to get the Strategy update data for.
        /// </summary>
        public string Symbol { get; }
        /// <summary>
        /// Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.
        /// </summary>
        public string StrategyID { get; }

        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        /// Entry progress value.
        /// </summary>
        public double EntryProgress { get; }

        /// <summary>
        /// Exit progress value.
        /// </summary>
        public double ExitProgress { get; }

        /// <summary>
        /// Entry signal for the strategy.
        /// </summary>
        public StrategySignal Signal { get; }

        /// <summary>
        /// Perception level. null if unset.
        /// </summary>
        public double? PerceptionLevel { get; }

        /// <summary>
        /// Signal tied to the perception level.
        /// </summary>
        public GaugeSignal PerceptionSignal { get; }

        /// <summary>
        /// Commitment level. null if unset.
        /// </summary>
        public double? CommitmentLevel { get; }

        /// <summary>
        /// Signal tied to the commitment level.
        /// </summary>
        public GaugeSignal CommitmentSignal { get; }

        /// <summary>
        /// Equilibrium level. null if unset.
        /// </summary>
        public double? EquilibriumLevel { get; }

        /// <summary>
        /// Signal tied to the equilibrium level.
        /// </summary>
        public GaugeSignal EquilibriumSignal { get; }

        /// <summary>
        /// Sentiment level. null if unset.
        /// </summary>
        public double? SentimentLevel { get; }

        /// <summary>
        /// Signal tied to the 50t sentiment indication.
        /// </summary>
        public GaugeSignal SentimentSignal { get; }

        public object Reference { get; }

        /// <summary>
        /// Holds error information, if a subscription error occured.
        /// </summary>
        public SubscriptionError Error { get; }

        /// <summary>
        /// Creates a new StrategyEventArgs instance.
        /// </summary>
        /// <param name="timeStamp">Timestamp of the latest update.</param>
        /// <param name="symbol">Symbol to get the Strategy update data for.</param>
        /// <param name="strategyID">Strategy to subscribe to. Example enum values: PPr4.0, BTr4.0, Crb.8.4.</param>
        /// <param name="entryProgress">Entry progress value.</param>
        /// <param name="exitProgress">Exit progress value.</param>
        /// <param name="perceptionLevel">Perception level. null if unset.</param>
        /// <param name="perceptionSignal">Signal tied to the perception level.</param>
        /// <param name="commitmentLevel">Commitment level. null if unset.</param>
        /// <param name="commitmentSignal">Signal tied to the commitment level.</param>
        /// <param name="equilibriumLevel">Equilibrium level. null if unset.</param>
        /// <param name="equilibriumSignal">Signal tied to the equilibrium level.</param>
        /// <param name="sentimentLevel">Sentiment level. null if unset.</param>
        /// <param name="sentimentSignal">Signal tied to the 50t sentiment indication.</param>
        /// <param name="signal">Entry signal for the strategy.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal StrategyEventArgs(DateTime timeStamp, string symbol, string strategyID, double entryProgress,
                                   double exitProgress, double? perceptionLevel, GaugeSignal perceptionSignal,
                                   double? commitmentLevel, GaugeSignal commitmentSignal,
                                   double? equilibriumLevel, GaugeSignal equilibriumSignal, double? sentimentLevel, 
                                   GaugeSignal sentimentSignal, StrategySignal signal, 
                                   object reference = null, SubscriptionError error = null)
        {
            TimeStamp = timeStamp;
            Symbol = symbol;
            StrategyID = strategyID;
            EntryProgress = entryProgress;
            ExitProgress = exitProgress;
            PerceptionLevel = perceptionLevel;
            PerceptionSignal = perceptionSignal;
            CommitmentLevel = commitmentLevel;
            CommitmentSignal = commitmentSignal;
            EquilibriumLevel = equilibriumLevel;
            EquilibriumSignal = equilibriumSignal;
            SentimentLevel = sentimentLevel;
            SentimentSignal = sentimentSignal;
            Signal = signal;
            Reference = reference;
            Error = error;
        }
    }
}
