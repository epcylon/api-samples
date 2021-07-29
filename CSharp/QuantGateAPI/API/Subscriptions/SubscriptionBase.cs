using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using Google.Protobuf;
using QuantGateAPI.API.Subscriptions;
using System;
using System.Windows.Threading;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public abstract class SubscriptionBase<M, V> : ProtoStompSubscription, ISubscription
        where M : IMessage<M>
        where V : ValueBase, new()
    {
        private readonly MessageParser<M> _parser;
        public V Values { get; }

        private readonly Dispatcher _dispatcher;

        public SubscriptionBase(ProtoStompClient client, MessageParser<M> parser,
                                string destination, bool receipt = false, uint throttleRate = 0) :
            base(client, destination, receipt, throttleRate)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _parser = parser;
            Values = new V() { Subscription = this };
            OnNext += HandleOnNext;
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                HandleUpdate(update, processed);
                Values.Updated();
            }));
        }

        protected virtual object Preprocess(M update) => null;
        protected abstract void HandleUpdate(M update, object processed);
    }
}
