using Google.Protobuf;
using QuantGate.API.Signals.ProtoStomp;
using System;
using System.Threading;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class SubscriptionBase<M, V> : ProtoStompSubscription, ISubscription<V>
        where M : IMessage<M>
        where V : EventArgs
    {
        private readonly MessageParser<M> _parser;
        public Subscription<V> External { get; }
        Subscription<V> ISubscription<V>.External => External;
        APIClient ISubscription<V>.Client => Client;

        public SubscriptionBase(APIClient client, MessageParser<M> parser,
                                string destination, bool receipt = false, uint throttleRate = 0) :
            base(client, destination, receipt, throttleRate)
        {
            _parser = parser;
            External = new Subscription<V>() { Source = this };
            OnNext += HandleOnNext;            
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);

            Client.Sync.Post(new SendOrPostCallback((o) =>
            {
                V updated = HandleUpdate(update, processed);
                External.SendUpdated(updated);
            }), null);
        }

        protected void PostUpdate(V update)
        {
            Client.Sync.Post(new SendOrPostCallback((o) =>
            {
                External.SendUpdated(update);
            }), null);
        }

        protected virtual object Preprocess(M update) => null;
        protected abstract V HandleUpdate(M update, object processed);
    }
}
