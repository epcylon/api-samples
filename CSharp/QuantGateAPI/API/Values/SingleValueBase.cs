namespace BridgeRock.CSharpExample.API.Values
{
    public class SingleValueBase : GaugeValueBase
    {
        private double _gaugeLevel;

        public double GaugeLevel
        {
            get => _gaugeLevel;
            set
            {
                if (_gaugeLevel != value)
                {
                    _gaugeLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

    public class BookPressure : SingleValueBase { }
    public class Headroom : SingleValueBase {}
    public class Perception : SingleValueBase { }
    public class Commitment : SingleValueBase { }
}
