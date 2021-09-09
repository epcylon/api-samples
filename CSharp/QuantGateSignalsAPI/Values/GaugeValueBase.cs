using System;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Base class for gauge values.
    /// </summary>
    public abstract class GaugeValueBase<V> : ValueBase
        where V : ValueBase
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
        /// The symbol being subscribed to for this gauge.
        /// </summary>
        public string Symbol { get; internal set; }

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
