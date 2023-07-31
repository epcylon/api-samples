using Google.Protobuf;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.ProtoStomp;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class SubscriptionBase : ProtoStompSubscription
    {
        public HashSet<object> References { get; }

        public SubscriptionBase(APIClient client, string destination, bool receipt = false,
                                uint throttleRate = 0, object reference = null) :
            base(client, destination, receipt, throttleRate)
        {
            if (reference is not null)
                References = new HashSet<object> { reference };
            else
                References = new HashSet<object>();
        }
    }

    internal abstract class SubscriptionBase<M, V> : SubscriptionBase
        where M : IMessage<M>
        where V : SubscriptionEventArgs
    {
        /// <summary>
        /// Empty object to use (instead of null).
        /// </summary>
        private readonly object _emptyObject = new();

        /// <summary>
        /// Notifies that the object was updated (through the parent).
        /// </summary>
        public EventHandler<V> ParentUpdatedEvent { get; }

        private readonly MessageParser<M> _parser;

        public SubscriptionBase(APIClient client, MessageParser<M> parser, EventHandler<V> handler,
                                string destination, bool receipt = false, uint throttleRate = 0, object reference = null) :
            base(client, destination, receipt, throttleRate, reference)
        {
            _parser = parser;
            ParentUpdatedEvent = handler;
            OnNext += HandleOnNext;
            OnError += HandleError;
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);

            Client.Sync.Post(new SendOrPostCallback(o =>
            {
                V updated = HandleUpdate(update, processed);
                SendUpdateToAll(updated);
            }),
            null);
        }

        private void HandleError(ProtoStompSubscription subscription, Exception exception)
        {
            // Get the error information.
            SubscriptionError error = new(exception.Message, exception.InnerException?.Message ?? string.Empty);
            // Wrap the error and send to handlers.
            PostUpdate(WrapError(error));
            // Unsubscribe this subscription.
            Unsubscribe();
        }

        protected void PostUpdate(V update)
        {
            Client.Sync.Post(new SendOrPostCallback(o =>
            {
                SendUpdateToAll(update);
            }), null);
        }

        private void SendUpdateToAll(V update)
        {
            if (References.Count == 0)
            {
                update.Reference = null;
                ParentUpdatedEvent?.Invoke(Client, update);
            }
            else
            {
                foreach (object reference in References.ToList())
                {
                    update = (V)update.Clone();
                    update.Reference = reference;
                    ParentUpdatedEvent?.Invoke(Client, update);
                }
            }
        }

        protected virtual object Preprocess(M update) => _emptyObject;
        protected abstract V HandleUpdate(M update, object processed);
        protected abstract V WrapError(SubscriptionError error);
    }
}
