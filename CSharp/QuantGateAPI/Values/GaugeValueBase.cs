using System;

namespace QuantGate.API.Values
{
    /// <summary>
    /// Base class for gauge values.
    /// </summary>
    public abstract class GaugeValueBase : ValueBase
    {
        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
        private DateTime _timestamp;
        /// <summary>
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// Timestamp of the latest update.
        /// </summary>
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

        /// <summary>
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </summary>
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
