using QuantGate.API.Utilities;
using QuantGate.API.Values;

namespace QuantGate.API.Subscriptions
{
    internal class PerceptionSubscription : SingleValueSubscription<Perception>
    {
        public PerceptionSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugePerception, streamID, symbol, null, receipt, throttleRate)
        {
        }
    }
}
