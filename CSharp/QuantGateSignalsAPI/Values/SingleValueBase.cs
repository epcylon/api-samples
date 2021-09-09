namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Base class for single value results.
    /// </summary>
    public class SingleValueBase<V> : GaugeValueBase<V>
        where V: ValueBase
    {
        /// <summary>
        /// The level of the gauge at the last update.
        /// </summary>
        private double _gaugeLevel;

        /// <summary>
        /// The level of the gauge at the last update.
        /// </summary>
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

    /// <summary>
    /// Holds Book Pressure values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class BookPressure : SingleValueBase<BookPressure> { }
    /// <summary>
    /// Holds Headroom values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Headroom : SingleValueBase<Headroom> {}
    /// <summary>
    /// Holds Perception values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Perception : SingleValueBase<Perception> { }
    /// <summary>
    /// Holds Commitment values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class Commitment : SingleValueBase<Commitment> { }
}
