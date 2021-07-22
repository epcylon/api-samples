using System;

namespace BridgeRock.CSharpExample.API.Values
{
    public abstract class GaugeValueBase : ValueBase
    {
        private DateTime _timestamp;
        private bool _isDirty;

        public DateTime TimeStamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp.Ticks != value.Ticks)
                {
                    _timestamp = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
