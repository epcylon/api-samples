namespace BridgeRock.CSharpExample.API.Values
{
    public class Sentiment : GaugeValueBase
    {
        public const int TotalBars = 55;

        private readonly double[] _lengths;
        private readonly double[] _colors;
        private double _avgLength;
        private double _avgColor;

        public Sentiment()
        {
            _lengths = new double[TotalBars];
            _colors = new double[TotalBars];
        }

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
