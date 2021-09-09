namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Base class for single value results.
    /// </summary>
    public class SingleValueBase : GaugeValueBase
    {
        /// <summary>
        /// The level of the gauge at the last update.
        /// </summary>
        public double Value { get; internal set; }
    }

    /// <summary>
    /// Holds Book Pressure values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class BookPressureEventArgs : SingleValueBase { }

    /// <summary>
    /// Holds Headroom values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class HeadroomEventArgs : SingleValueBase {}
    /// <summary>
    /// Holds Perception values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class PerceptionEventArgs : SingleValueBase { }
    /// <summary>
    /// Holds Commitment values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class CommitmentEventArgs : SingleValueBase { }
}
