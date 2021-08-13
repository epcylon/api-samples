using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal interface ISubscription
    {
        void Subscribe();
        void Unsubscribe();
        ValueBase Values { get; }
        uint ThrottleRate { get; set; }
    }
}
