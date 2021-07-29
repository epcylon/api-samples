using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    internal class CommitmentSubscription : SingleValueSubscription<Commitment>
    {
        public CommitmentSubscription(ProtoStompClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeCommitment, streamID, symbol, "1m", receipt, throttleRate)
        {
        }
    }
}
