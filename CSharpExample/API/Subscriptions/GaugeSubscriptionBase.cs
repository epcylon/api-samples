using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using Google.Protobuf;
using System;
using System.Windows.Threading;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public abstract class GaugeSubscriptionBase<M, V> : ProtoStompSubscription
        where M : IMessage<M>
        where V : GaugeValueBase, new()
    {
        private readonly MessageParser<M> _parser;
        public V Values {get;} = new V();

        private Dispatcher _dispatcher;

        public GaugeSubscriptionBase(ProtoStompClient client, MessageParser<M> parser, SubscriptionPath gaugePath, string streamID,
                                     string symbol, string compression = null, bool receipt  = false, uint throttleRate = 0) :
            base(client, ParsedDestination.CreateGaugeDestination(gaugePath, streamID, symbol, compression).Destination, receipt, throttleRate)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _parser = parser;
            OnNext += HandleOnNext;
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            M update = _parser.ParseFrom(values);
            object processed = Preprocess(update);
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action<M, object>)HandleUpdate, update, processed);
        }

        protected virtual object Preprocess(M update) => null;
        protected abstract void HandleUpdate(M update, object processed);
    }
}
