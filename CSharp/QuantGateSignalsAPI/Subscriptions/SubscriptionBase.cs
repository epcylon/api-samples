using Google.Protobuf;
using QuantGate.API.Signals.ProtoStomp;
using System;
using System.Threading;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class SubscriptionBase<M, V> : ProtoStompSubscription
        where M : IMessage<M>
        where V : EventArgs
    {
        /// <summary>
        /// Notifies that the object was updated (through the parent).
        /// </summary>
        public EventHandler<V> ParentUpdatedEvent { get; set; }

        private readonly MessageParser<M> _parser;        

        public SubscriptionBase(APIClient client, MessageParser<M> parser,
                                string destination, bool receipt = false, uint throttleRate = 0) :
            base(client, destination, receipt, throttleRate)
        {
            _parser = parser;
            OnNext += HandleOnNext;            
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);

            Client.Sync.Post(new SendOrPostCallback((o) =>
            {
                V updated = HandleUpdate(update, processed);
                ParentUpdatedEvent?.Invoke(Client, updated);
            }), null);
        }

        protected void PostUpdate(V update)
        {
            Client.Sync.Post(new SendOrPostCallback((o) =>
            {
                ParentUpdatedEvent?.Invoke(Client, update);
            }), null);
        }

        protected virtual object Preprocess(M update) => null;
        protected abstract V HandleUpdate(M update, object processed);
    }
}
