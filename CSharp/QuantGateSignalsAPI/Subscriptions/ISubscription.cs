using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal interface ISubscription<V>
        where V : EventArgs
    {
        void Subscribe();
        void Unsubscribe();
        APIClient Client { get; }
        Subscription<V> External { get; }
        uint ThrottleRate { get; set; }
    }
}
