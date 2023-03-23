namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Trigger values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class TriggerEventArgs : GaugeEventArgs
    {
        /// <summary>
        /// Bias value.
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// Perception value.
        /// </summary>
        public double Perception { get; }

        /// <summary>
        /// Sentiment length value at point 0 (center).
        /// </summary>
        public double Sentiment { get; }

        /// <summary>
        /// Commitment value.
        /// </summary>
        public double Commitment { get; }

        /// <summary>
        /// The Equilibrium Price.
        /// </summary>
        public double EquilibriumPrice { get; }

        /// <summary>
        /// Gap size of each equilibrium deviation.
        /// </summary>
        public double GapSize { get; }

        /// <summary>
        /// Last traded price at the time of calculation.
        /// </summary>
        public double LastPrice { get; }

        /// <summary>
        /// Creates a new TriggerEventArgs instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="perception">Perception value.</param>
        /// <param name="commitment">Commitment value.</param>
        /// <param name="sentiment">Sentiment length value at point 0 (center).</param>
        /// <param name="equilibriumPrice">The Equilibrium Price.</param>
        /// <param name="gapSize">Gap size of each equilibrium deviation.</param>
        /// <param name="lastPrice">Last traded price at the time of calculation.</param>
        /// <param name="bias">Bias value.</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal TriggerEventArgs(string symbol, DataStream stream, DateTime timestamp, double perception,
                                  double commitment, double sentiment, double equilibriumPrice, double gapSize,
                                  double lastPrice, double bias, bool isDirty, SubscriptionError error = null) :
            base(symbol, stream, timestamp, isDirty, error)
        {
            Perception = perception;
            Commitment = commitment;
            Sentiment = sentiment;
            EquilibriumPrice = equilibriumPrice;
            GapSize = gapSize;
            LastPrice = lastPrice;
            Bias = bias;
        }

        /// <summary>
        /// The current equilibrium gauge level in standard deviations from the equilibrium price.
        /// </summary>
        public double EquilibriumSTD
        {
            get
            {
                if (LastPrice == 0 || EquilibriumPrice == 0 || GapSize == 0)
                    return 0;

                return (LastPrice - EquilibriumPrice) / GapSize;
            }
        }

        /// <summary>
        /// Returns the equilibrium band price at the given level of standard deviations.
        /// </summary>
        /// <param name="level">The level of standard deviations above or below the equilibrium price to calculate.</param>
        /// <returns>The equilibrium band price at the given level of standard deviations.</returns>
        public double EquilibriumBand(double level) => EquilibriumPrice + GapSize * level;
    }
}
