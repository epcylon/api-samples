namespace QuantGate.API.Values
{
    /// <summary>
    /// Holds Sentiment values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Sentiment : GaugeValueBase
    {
        /// <summary>
        /// The total number of bars in the Sentiment gauge.
        /// </summary>
        public const int TotalBars = 55;

        /// <summary>
        /// Holds the lengths of each bar.
        /// </summary>
        private readonly double[] _lengths;
        /// <summary>
        /// Holds the colors of each bar.
        /// </summary>
        private readonly double[] _colors;
        /// <summary>
        /// Average bar length.
        /// </summary>
        private double _avgLength;
        /// <summary>
        /// Average bar color.
        /// </summary>
        private double _avgColor;

        /// <summary>
        /// Creates a new Sentiment instance.
        /// </summary>
        public Sentiment()
        {
            _lengths = new double[TotalBars];
            _colors = new double[TotalBars];
        }

        /// <summary>
        /// Holds the lengths of each bar.
        /// </summary>
        public double[] Lengths
        {
            get => _lengths;
            set
            {
                bool changed = false;

                if (value.Length == TotalBars)
                {
                    for (int index = 0; index < TotalBars; index++)
                    {
                        if (_lengths[index] != value[index])
                        {
                            _lengths[index] = value[index];
                            changed = true;
                        }
                    }

                    if (changed)
                        NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Holds the colors of each bar.
        /// </summary>
        public double[] Colors
        {
            get => _colors;
            set
            {
                bool changed = false;

                if (value.Length == TotalBars)
                {
                    for (int index = 0; index < TotalBars; index++)
                    {
                        if (_colors[index] != value[index])
                        {
                            _colors[index] = value[index];
                            changed = true;
                        }
                    }

                    if (changed)
                        NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Average bar length.
        /// </summary>
        public double AvgLength
        {
            get => _avgLength;
            set
            {
                if (_avgLength != value)
                {
                    _avgLength = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Average bar color.
        /// </summary>
        public double AvgColor
        {
            get => _avgColor;
            set
            {
                if (_avgColor != value)
                {
                    _avgColor = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
