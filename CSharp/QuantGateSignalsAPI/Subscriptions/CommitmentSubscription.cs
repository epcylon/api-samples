using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class CommitmentSubscription : SingleValueSubscription<CommitmentEventArgs>
    {
        public CommitmentSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeCommitment, streamID, symbol, "1m", receipt, throttleRate)
        {
        }
    }
}
