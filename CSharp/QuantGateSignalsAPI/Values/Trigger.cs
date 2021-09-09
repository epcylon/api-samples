namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Trigger values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Trigger : GaugeValueBase<Trigger>
    {
        /// <summary>
        /// Bias value.
        /// </summary>
        public double Bias { get; internal set; }

        /// <summary>
        /// Perception value.
        /// </summary>
        public double Perception { get; internal set; }

        /// <summary>
        /// Sentiment length value at point 0.
        /// </summary>
        public double Sentiment { get; internal set; }

        /// <summary>
        /// Commitment value.
        /// </summary>
        public double Commitment { get; internal set; }

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

        /// <summary>
        /// The Equilibrium Price.
        /// </summary>
        public double EquilibriumPrice { get; internal set; }

        /// <summary>
        /// Gap size of each equilibrium deviation.
        /// </summary>
        public double GapSize { get; internal set; }

        /// <summary>
        /// Last traded price at the time of calculation.
        /// </summary>
        public double LastPrice { get; internal set; }
    }
}
