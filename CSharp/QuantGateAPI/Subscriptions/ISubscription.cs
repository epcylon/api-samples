using QuantGate.API.Values;

namespace QuantGate.API.Subscriptions
{
    internal interface ISubscription
    {
        void Subscribe();
        void Unsubscribe();
        ValueBase Values { get; }
    }
}
