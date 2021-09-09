namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Equilibrium values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Equilibrium : GaugeValueBase<Equilibrium>
    {
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

        /// <summary>
        /// Position of the high value.
        /// </summary>
        public double High { get; internal set; }

        /// <summary>
        /// Position of the low value.
        /// </summary>
        public double Low { get; internal set; }

        /// <summary>
        /// Position of the projected value.
        /// </summary>
        public double Projected { get; internal set; }

        /// <summary>
        /// Bias (as determined by the slope).
        /// </summary>
        public double Bias { get; internal set; }
    }
}
