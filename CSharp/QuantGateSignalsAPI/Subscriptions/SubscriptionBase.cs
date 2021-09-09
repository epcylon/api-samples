using Google.Protobuf;
using QuantGate.API.Signals.ProtoStomp;
using QuantGate.API.Signals.Values;
using System.Threading;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class SubscriptionBase<M, V> : ProtoStompSubscription, ISubscription<V>
        where M : IMessage<M>
        where V : ValueBase, new()
    {
        private readonly MessageParser<M> _parser;
        public SignalStream<V> Stream { get; }
        SignalStream<V> ISubscription<V>.Stream => Stream;
        APIClient ISubscription<V>.Client => Client;

        public SubscriptionBase(APIClient client, MessageParser<M> parser,
                                string destination, bool receipt = false, uint throttleRate = 0) :
            base(client, destination, receipt, throttleRate)
        {
            _parser = parser;
            Stream = new SignalStream<V>() { Subscription = this };
            OnNext += HandleOnNext;
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);

            Client.Sync.Post(new SendOrPostCallback((o) =>
            {
                V updated = HandleUpdate(update, processed);
                Stream.SendUpdated(updated);
            }), null);
        }

        protected virtual object Preprocess(M update) => null;
        protected abstract V HandleUpdate(M update, object processed);
    }
}
