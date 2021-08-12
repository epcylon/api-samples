using Google.Protobuf;
using QuantGate.API.ProtoStomp;
using QuantGate.API.Values;
using System.Threading;

namespace QuantGate.API.Subscriptions
{
    internal abstract class SubscriptionBase<M, V> : ProtoStompSubscription, ISubscription
        where M : IMessage<M>
        where V : ValueBase, new()
    {
        private readonly MessageParser<M> _parser;
        public V Values { get; }
        ValueBase ISubscription.Values => Values;

        public SubscriptionBase(APIClient client, MessageParser<M> parser,
                                string destination, bool receipt = false, uint throttleRate = 0) :
            base(client, destination, receipt, throttleRate)
        {
            _parser = parser;
            Values = new V() { Subscription = this };
            OnNext += HandleOnNext;
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);

            Client.Sync.Post(new SendOrPostCallback((o) =>
            {
                HandleUpdate(update, processed);
                Values.SendUpdated();
            }), null);
        }

        protected virtual object Preprocess(M update) => null;
        protected abstract void HandleUpdate(M update, object processed);
    }
}
