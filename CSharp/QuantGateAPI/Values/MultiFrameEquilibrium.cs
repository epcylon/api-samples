namespace QuantGate.API.Values
{
    public class MultiframeEquilibrium : GaugeValueBase
    {
        private double _min5;
        private double _min10;
        private double _min15;
        private double _min30;
        private double _min45;
        private double _min60;
        private double _min120;
        private double _min180;
        private double _min240;
        private double _day1;

        public double Min5
        {
            get => _min5;
            set
            {
                if (_min5 != value)
                {
                    _min5 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min10
        {
            get => _min10;
            set
            {
                if (_min10 != value)
                {
                    _min10 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min15
        {
            get => _min15;
            set
            {
                if (_min15 != value)
                {
                    _min15 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min30
        {
            get => _min30;
            set
            {
                if (_min30 != value)
                {
                    _min30 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min45
        {
            get => _min45;
            set
            {
                if (_min45 != value)
                {
                    _min45 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min60
        {
            get => _min60;
            set
            {
                if (_min60 != value)
                {
                    _min60 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min120
        {
            get => _min120;
            set
            {
                if (_min120 != value)
                {
                    _min120 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min180
        {
            get => _min180;
            set
            {
                if (_min180 != value)
                {
                    _min180 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Min240
        {
            get => _min240;
            set
            {
                if (_min240 != value)
                {
                    _min240 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Day1
        {
            get => _day1;
            set
            {
                if (_day1 != value)
                {
                    _day1 = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
