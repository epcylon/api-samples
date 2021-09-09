namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Sentiment values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Sentiment : GaugeValueBase<Sentiment>
    {
        /// <summary>
        /// The total number of bars in the Sentiment gauge.
        /// </summary>
        public const int TotalBars = 55;

        /// <summary>
        /// Creates a new Sentiment instance.
        /// </summary>        
        public Sentiment()
        {
            Lengths = new double[TotalBars];
            Colors = new double[TotalBars];
        }

        /// <summary>
        /// Holds the lengths of each bar.
        /// </summary>
        public double[] Lengths { get; internal set; }

        /// <summary>
        /// Holds the colors of each bar.
        /// </summary>
        public double[] Colors { get; internal set; }

        /// <summary>
        /// Average bar length.
        /// </summary>
        public double AvgLength { get; internal set; }

        /// <summary>
        /// Average bar color.
        /// </summary>
        public double AvgColor { get; internal set; }
    }
}
