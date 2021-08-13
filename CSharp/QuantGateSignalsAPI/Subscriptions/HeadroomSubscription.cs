using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class HeadroomSubscription : SingleValueSubscription<Headroom>
    {
        public HeadroomSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeHeadroom, streamID, symbol, "5m", receipt, throttleRate)
        {
        }
    }
}
