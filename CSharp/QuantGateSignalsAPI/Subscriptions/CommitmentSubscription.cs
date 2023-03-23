using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class CommitmentSubscription : GaugeSubscriptionBase<SingleValueUpdate, CommitmentEventArgs>
    {
        public CommitmentSubscription(APIClient client, EventHandler<CommitmentEventArgs> handler,
                                      string streamID, string symbol, bool receipt = false,
                                      uint throttleRate = 0, object reference = null) :
            base(client, SingleValueUpdate.Parser, handler, SubscriptionPath.GaugeCommitment,
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, "1m",
                 receipt, throttleRate, reference)
        {
        }

        protected override CommitmentEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {
            return new CommitmentEventArgs(
                Symbol, Stream,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }

        protected override CommitmentEventArgs WrapError(SubscriptionError error) =>
            new(Symbol, Stream, DateTime.UtcNow, 0, true, error);
    }
}
