using QuantGate.API.Utilities;
using QuantGate.API.Values;

namespace QuantGate.API.Subscriptions
{
    internal class CommitmentSubscription : SingleValueSubscription<Commitment>
    {
        public CommitmentSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeCommitment, streamID, symbol, "1m", receipt, throttleRate)
        {
        }
    }
}
