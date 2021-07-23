using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class PerceptionSubscription : SingleValueSubscription<Perception>
    {
        public PerceptionSubscription(ProtoStompClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugePerception, streamID, symbol, null, receipt, throttleRate)
        {
        }
    }
}
