using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal interface ISubscription<V>
        where V : ValueBase
    {
        void Subscribe();
        void Unsubscribe();
        APIClient Client { get; }
        SignalStream<V> Stream { get; }
        uint ThrottleRate { get; set; }
    }
}
