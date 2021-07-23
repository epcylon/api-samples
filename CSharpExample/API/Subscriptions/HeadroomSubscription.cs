using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class HeadroomSubscription : SingleValueSubscription<Headroom>
    {
        public HeadroomSubscription(ProtoStompClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeHeadroom, streamID, symbol, "5m", receipt, throttleRate)
        {
        }
    }
}
