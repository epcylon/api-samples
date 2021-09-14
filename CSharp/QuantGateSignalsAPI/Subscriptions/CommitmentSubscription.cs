using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class CommitmentSubscription : GaugeSubscriptionBase<SingleValueUpdate, CommitmentEventArgs>
    {
        public CommitmentSubscription(APIClient client, string streamID, string symbol, 
                                      bool receipt = false, uint throttleRate = 0) :
            base(client, SingleValueUpdate.Parser, SubscriptionPath.GaugeCommitment, 
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, "1m", receipt, throttleRate)
        {
        }

        protected override CommitmentEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {
            return new CommitmentEventArgs(
                Symbol,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }
    }
}
