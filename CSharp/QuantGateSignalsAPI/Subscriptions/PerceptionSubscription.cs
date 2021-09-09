using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class PerceptionSubscription : SingleValueSubscription<PerceptionEventArgs>
    {
        public PerceptionSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugePerception, streamID, symbol, null, receipt, throttleRate)
        {
        }
    }
}
