using QuantGate.API.Signals.Subscriptions;
using QuantGate.API.Signals.Values;
using System;

namespace QuantGate.API.Signals
{
    public class Subscription<V>
        where V : ValueBase
    {
        /// <summary>
        /// Notifies that the object was updated (after complete update).
        /// </summary>
        public event EventHandler<V> Updated;

        /// <summary>
        /// Notifies that the object was updated (through the parent).
        /// </summary>
        internal EventHandler<V> ParentUpdatedEvent;

        /// <summary>
        /// Holds a reference to the subscription that these values are streamed from.
        /// </summary>
        internal ISubscription<V> Source { get; set; }

        /// <summary>
        /// Called whenever the values are finished updating.
        /// </summary>
        internal void SendUpdated(V values)
        {
            Updated?.Invoke(Source.Client, values);
            ParentUpdatedEvent?.Invoke(Source.Client, values);
        }

        /// <summary>
        /// Unsubscribe from the subscription.
        /// </summary>
        internal void Unsubscribe() => Source.Unsubscribe();

        /// <summary>
        /// The throttle rate of the subscription for these values (in ms).
        /// </summary>
        /// <remarks>Setting this value will change the throttle rate.</remarks>
        public int ThrottleRate
        {
            get => (int)Source.ThrottleRate;
            set => Source.ThrottleRate = (uint)value;
        }
    }
}
