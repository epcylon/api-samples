namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Sentiment values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class SentimentEventArgs : GaugeEventArgs
    {
        /// <summary>
        /// The total number of bars in the Sentiment gauge.
        /// </summary>
        public const int TotalBars = 55;

        /// <summary>
        /// Compression timeframe to apply to the gauge. Default value is 50t.
        /// </summary>
        public string Compression { get; }

        /// <summary>
        /// Holds the lengths of each bar.
        /// </summary>
        public IReadOnlyList<double> Lengths { get; }

        /// <summary>
        /// Holds the colors of each bar.
        /// </summary>
        public IReadOnlyList<double> Colors { get; }

        /// <summary>
        /// Average bar length.
        /// </summary>
        public double AvgLength { get; }

        /// <summary>
        /// Average bar color.
        /// </summary>
        public double AvgColor { get; }

        /// <summary>
        /// Length I index.
        /// </summary>
        public int LengthI { get; }
        /// <summary>
        /// Length J index.
        /// </summary>
        public int LengthJ { get; }
        /// <summary>
        /// Color I index.
        /// </summary>
        public int ColorI { get; }
        /// <summary>
        /// Length J index.
        /// </summary>
        public int ColorJ { get; }

        /// <summary>
        /// Creates a new SentimentEventArgs instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="compression">Compression timeframe to apply to the gauge. Default value is 50t.</param>
        /// <param name="avgLength">Average bar length.</param>
        /// <param name="lengthI">Length I index.</param>
        /// <param name="lengthJ">Length J index.</param>
        /// <param name="lengths">Holds the lengths of each bar.</param>
        /// <param name="avgColor">Average bar color.</param>
        /// <param name="colorI">Color I index.</param>
        /// <param name="colorJ">Color J index.</param>
        /// <param name="colors">Holds the colors of each bar.</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal SentimentEventArgs(string symbol, DataStream stream, DateTime timestamp, string compression,
                                    double avgLength, int lengthI, int lengthJ, double[] lengths, double avgColor,
                                    int colorI, int colorJ, double[] colors, bool isDirty, SubscriptionError error = null) :
            base(symbol, stream, timestamp, isDirty, error)
        {
            Compression = compression;
            AvgLength = avgLength;
            LengthI = lengthI;
            LengthJ = lengthJ;
            Lengths = lengths;
            AvgColor = avgColor;
            ColorI = colorI;
            ColorJ = colorJ;
            Colors = colors;
        }
    }
}
