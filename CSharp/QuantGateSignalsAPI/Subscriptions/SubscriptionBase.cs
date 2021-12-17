using Google.Protobuf;
using QuantGate.API.Signals.Events;
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
        /// Empty object to use (instead of null).
        /// </summary>
        private readonly object _emptyObject = new object();

        /// <summary>
        /// Notifies that the object was updated (through the parent).
        /// </summary>
        public EventHandler<V> ParentUpdatedEvent { get; }

        private readonly MessageParser<M> _parser;

        public SubscriptionBase(APIClient client, MessageParser<M> parser, EventHandler<V> handler,
                                string destination, bool receipt = false, uint throttleRate = 0) :
            base(client, destination, receipt, throttleRate)
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
                ParentUpdatedEvent?.Invoke(Client, updated);
            }), null);
        }

        private void HandleError(ProtoStompSubscription subscription, Exception exception)
        {
            // Get the error information.
            SubscriptionError error =
                new SubscriptionError(exception.Message, exception.InnerException?.Message ?? string.Empty);
            // Wrap the error and send to handlers.
            PostUpdate(WrapError(error));
            // Unsubscribe this subscription.
            Unsubscribe();
        }

        protected void PostUpdate(V update)
        {
            Client.Sync.Post(new SendOrPostCallback(o =>
            {
                ParentUpdatedEvent?.Invoke(Client, update);
            }), null);
        }

        protected virtual object Preprocess(M update) => _emptyObject;
        protected abstract V HandleUpdate(M update, object processed);
        protected abstract V WrapError(SubscriptionError error);
    }
}
