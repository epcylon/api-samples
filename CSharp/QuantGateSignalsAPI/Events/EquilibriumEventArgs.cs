namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Equilibrium values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class EquilibriumEventArgs : GaugeEventArgs
    {
        /// <summary>
        /// Compression timeframe to apply to the gauge. Default value is 300s.
        /// </summary>
        public string Compression { get; }

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
        /// Position of the high value.
        /// </summary>
        public double High { get; }

        /// <summary>
        /// Position of the low value.
        /// </summary>
        public double Low { get; }

        /// <summary>
        /// Bias (as determined by the slope).
        /// </summary>
        public double Bias { get; }

        /// <summary>
        /// The projected equilibrium price.
        /// </summary>
        public double Projected { get; }

        /// <summary>
        /// The current projected equilibrium level in standard deviations from the equilibrium price.
        /// </summary>
        public double ProjectedSTD { get; }

        /// <summary>
        /// Creates a new EquilibriumEventArgs instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 300s.</param>
        /// <param name="equilibriumPrice">The Equilibrium Price.</param>
        /// <param name="gapSize">Gap size of each equilibrium deviation.</param>
        /// <param name="lastPrice">Last traded price at the time of calculation.</param>
        /// <param name="high">Position of the high value.</param>
        /// <param name="low">Position of the low value.</param>
        /// <param name="projectedPosition">Position of the projected value.</param>
        /// <param name="bias">Bias (as determined by the slope).</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal EquilibriumEventArgs(string symbol, DataStream stream, DateTime timestamp, string compression,
                                      double equilibriumPrice, double gapSize, double lastPrice, double high, double low,
                                      double projectedPosition, double bias, bool isDirty, SubscriptionError error = null) :
            base(symbol, stream, timestamp, isDirty, error)
        {
            Compression = compression;
            EquilibriumPrice = equilibriumPrice;
            GapSize = gapSize;
            LastPrice = lastPrice;
            High = high;
            Low = low;
            ProjectedSTD = projectedPosition / 200.0;
            Projected = equilibriumPrice + ProjectedSTD * gapSize;
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
