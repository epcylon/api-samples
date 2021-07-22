using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using Google.Protobuf;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public abstract class GaugeSubscriptionBase<M, V> : ProtoStompSubscription
        where M : IMessage<M>
        where V : GaugeValueBase, new()
    {
        private readonly MessageParser<M> _parser;
        public V Values {get;} = new V();

        public GaugeSubscriptionBase(ProtoStompClient client, MessageParser<M> parser, SubscriptionPath gaugePath, string streamID,
                                     string symbol, string compression = null, bool receipt  = false, uint throttleRate = 0) :
            base(client, ParsedDestination.CreateGaugeDestination(gaugePath, streamID, symbol, compression).Destination, receipt, throttleRate)
        {
            _parser = parser;
            OnNext += HandleOnNext;
        }

        private void HandleOnNext(ProtoStompSubscription subscription, ByteString values)
        {
            HandleUpdate(_parser.ParseFrom(values));
        }

        protected abstract void HandleUpdate(M update);
    }
}
