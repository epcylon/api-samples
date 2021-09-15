using System;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// The details of an individual symbol in response to a top symbols request.
    /// </summary>
    public class TopSymbol : SearchResult
    {
        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        public DateTime Timestamp { get; }
        /// <summary>
        /// Entry progress value.
        /// </summary>
        public double EntryProgress { get; }
        /// <summary>
        /// Signal tied to the Perception level.
        /// </summary>
        public GaugeSignal PerceptionSignal { get; }
        /// <summary>
        /// Signal tied to the Commitment level.
        /// </summary>
        public GaugeSignal CommitmentSignal { get; }
        /// <summary>
        /// Signal tied to the Equilibrium level.
        /// </summary>
        public GaugeSignal EquilibriumSignal { get; }
        /// <summary>
        /// Signal tied to the Sentiment level.
        /// </summary>
        public GaugeSignal SentimentSignal { get; }
        /// <summary>
        /// Entry signal for the default strategy.
        /// </summary>
        public StrategySignal Signal { get; }

        /// <summary>
        /// Creates a new TopSymbol instance.
        /// </summary>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="underlying">Underlying symbol.</param>
        /// <param name="currency">Currency the instrument is traded in.</param>
        /// <param name="instrumentType">Type of instrument.</param>
        /// <param name="exchange">Primary exchange (ISO 10383 MIC) the instrument is traded on.</param>
        /// <param name="displayName">Display name of the instrument.</param>
        /// <param name="entryProgress">Entry progress value.</param>
        /// <param name="perceptionSignal">Signal tied to the Perception level.</param>
        /// <param name="commitmentSignal">Signal tied to the Commitment level.</param>
        /// <param name="equilibriumSignal">Signal tied to the Equilibrium level.</param>
        /// <param name="sentimentSignal">Signal tied to the Sentiment level.</param>
        /// <param name="signal">Entry signal for the default strategy.</param>
        internal TopSymbol(DateTime timestamp, string symbol, string underlying, string currency,
                           InstrumentType instrumentType, string exchange, string displayName,
                           double entryProgress, GaugeSignal perceptionSignal, 
                           GaugeSignal commitmentSignal, GaugeSignal equilibriumSignal, 
                           GaugeSignal sentimentSignal, StrategySignal signal) :
            base(symbol, underlying, currency, instrumentType, exchange, displayName)
        {
            Timestamp = timestamp;
            EntryProgress = entryProgress;
            PerceptionSignal = perceptionSignal;
            CommitmentSignal = commitmentSignal;
            EquilibriumSignal = equilibriumSignal;
            SentimentSignal = sentimentSignal;
            Signal = signal;
        }
    }
}
