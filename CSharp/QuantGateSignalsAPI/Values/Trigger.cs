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
        private double _bias;
        /// <summary>
        /// Perception value.
        /// </summary>
        private double _perception;
        /// <summary>
        /// Sentiment length value at point 0.
        /// </summary>
        private double _sentiment;
        /// <summary>
        /// Commitment value.
        /// </summary>
        private double _commitment;
        /// <summary>
        /// The Equilibrium Price.
        /// </summary>
        private double _equilibriumPrice;
        /// <summary>
        /// Gap size of each equilibrium deviation.
        /// </summary>
        private double _gapSize;
        /// <summary>
        /// Last traded price at the time of calculation.
        /// </summary>
        private double _lastPrice;

        /// <summary>
        /// Bias value.
        /// </summary>
        public double Bias
        {
            get => _bias;
            set
            {
                if (_bias != value)
                {
                    _bias = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Perception value.
        /// </summary>
        public double Perception
        {
            get => _perception;
            set
            {
                if (_perception != value)
                {
                    _perception = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sentiment length value at point 0.
        /// </summary>
        public double Sentiment
        {
            get => _sentiment;
            set
            {
                if (_sentiment != value)
                {
                    _sentiment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Commitment value.
        /// </summary>
        public double Commitment
        {
            get => _commitment;
            set
            {
                if (_commitment != value)
                {
                    _commitment = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The current equilibrium gauge level in standard deviations from the equilibrium price.
        /// </summary>
        public double EquilibriumSTD
        {
            get
            {
                if (_lastPrice == 0 || _equilibriumPrice == 0 || _gapSize == 0)
                    return 0;

                return (_lastPrice - _equilibriumPrice) / _gapSize;
            }
        }

        /// <summary>
        /// Returns the equilibrium band price at the given level of standard deviations.
        /// </summary>
        /// <param name="level">The level of standard deviations above or below the equilibrium price to calculate.</param>
        /// <returns>The equilibrium band price at the given level of standard deviations.</returns>
        public double EquilibriumBand(double level) =>
            _equilibriumPrice + _gapSize * level;

        /// <summary>
        /// The Equilibrium Price.
        /// </summary>
        public double EquilibriumPrice
        {
            get => _equilibriumPrice;
            set
            {
                if (_equilibriumPrice != value)
                {
                    _equilibriumPrice = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gap size of each equilibrium deviation.
        /// </summary>
        public double GapSize
        {
            get => _gapSize;
            set
            {
                if (_gapSize != value)
                {
                    _gapSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Last traded price at the time of calculation.
        /// </summary>
        public double LastPrice
        {
            get => _lastPrice;
            set
            {
                if (_lastPrice != value)
                {
                    _lastPrice = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
