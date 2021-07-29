using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using Google.Protobuf;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    internal abstract class GaugeSubscriptionBase<M, V> : SubscriptionBase<M, V>
        where M : IMessage<M>
        where V : GaugeValueBase, new()
    {
        public GaugeSubscriptionBase(ProtoStompClient client, MessageParser<M> parser, SubscriptionPath gaugePath, string streamID,
                                     string symbol, string compression = null, bool receipt = false, uint throttleRate = 0) :
            base(client, parser, ParsedDestination.CreateGaugeDestination
                    (gaugePath, streamID, symbol, compression).Destination, receipt, throttleRate)
        {
        }
    }
}
