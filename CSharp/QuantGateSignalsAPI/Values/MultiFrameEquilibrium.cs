namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Multiframe Equilibrium values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class MultiframeEquilibrium : GaugeValueBase
    {
        /// <summary>
        /// 5 minute value.
        /// </summary>
        private double _min5;
        /// <summary>
        /// 10 minute value.
        /// </summary>
        private double _min10;
        /// <summary>
        /// 15 minute value.
        /// </summary>
        private double _min15;
        /// <summary>
        /// 30 minute value.
        /// </summary>
        private double _min30;
        /// <summary>
        /// 45 minute value.
        /// </summary>
        private double _min45;
        /// <summary>
        /// 60 minute value.
        /// </summary>
        private double _min60;
        /// <summary>
        /// 120 minute value.
        /// </summary>
        private double _min120;
        /// <summary>
        /// 180 minute value.
        /// </summary>
        private double _min180;
        /// <summary>
        /// 240 minute value.
        /// </summary>
        private double _min240;
        /// <summary>
        /// 1 day value.
        /// </summary>
        private double _day1;

        /// <summary>
        /// 5 minute value.
        /// </summary>
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

        /// <summary>
        /// 10 minute value.
        /// </summary>
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

        /// <summary>
        /// 15 minute value.
        /// </summary>
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

        /// <summary>
        /// 30 minute value.
        /// </summary>
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

        /// <summary>
        /// 45 minute value.
        /// </summary>
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

        /// <summary>
        /// 60 minute value.
        /// </summary>
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

        /// <summary>
        /// 120 minute value.
        /// </summary>
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

        /// <summary>
        /// 180 minute value.
        /// </summary>
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

        /// <summary>
        /// 240 minute value.
        /// </summary>
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

        /// <summary>
        /// 1 day value.
        /// </summary>
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
