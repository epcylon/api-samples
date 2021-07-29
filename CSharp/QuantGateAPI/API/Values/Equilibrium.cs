namespace BridgeRock.CSharpExample.API.Values
{
    public class Equilibrium : GaugeValueBase
    {
        private double _equilibriumPrice;
        private double _gapSize;
        private double _lastPrice;
        private double _high;
        private double _low;
        private double _projected;
        private double _bias;

        public double EquilibriumSTD
        {
            get
            {
                if (_lastPrice == 0 || _equilibriumPrice == 0 || _gapSize == 0)
                    return 0;

                return (_lastPrice - _equilibriumPrice) / _gapSize;
            }
        }

        public double EquilibriumLevel(double level) => 
            _equilibriumPrice + _gapSize * level;        

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
