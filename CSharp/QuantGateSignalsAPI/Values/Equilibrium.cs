namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Equilibrium values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Equilibrium : GaugeValueBase<Equilibrium>
    {
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
        /// Position of the high value.
        /// </summary>
        private double _high;
        /// <summary>
        /// Position of the low value.
        /// </summary>
        private double _low;
        /// <summary>
        /// Position of the projected value.
        /// </summary>
        private double _projected;
        /// <summary>
        /// Bias(determined by the slope).
        /// </summary>
        private double _bias;

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

        /// <summary>
        /// Position of the high value.
        /// </summary>
        public double High
        {
            get => _high;
            set
            {
                if (_high != value)
                {
                    _high = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Position of the low value.
        /// </summary>
        public double Low
        {
            get => _low;
            set
            {
                if (_low != value)
                {
                    _low = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Position of the projected value.
        /// </summary>
        public double Projected
        {
            get => _projected;
            set
            {
                if (_projected != value)
                {
                    _projected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Bias (as determined by the slope).
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
    }
}
