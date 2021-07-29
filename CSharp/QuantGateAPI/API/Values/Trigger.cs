namespace BridgeRock.CSharpExample.API.Values
{
    public class Trigger : GaugeValueBase
    {
        private double _bias;
        private double _perception;
        private double _sentiment;
        private double _commitment;
        private double _equilibriumPrice;
        private double _gapSize;
        private double _lastPrice;

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

        public double PerceptionPrice
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

        public double SentimentPrice
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

        public double CommitmentPrice
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
    }
}
