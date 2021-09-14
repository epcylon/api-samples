using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class PerceptionSubscription : GaugeSubscriptionBase<SingleValueUpdate, PerceptionEventArgs>
    {
        public PerceptionSubscription(APIClient client, string streamID, string symbol, 
                                      bool receipt = false, uint throttleRate = 0) :
            base(client, SingleValueUpdate.Parser, SubscriptionPath.GaugePerception, 
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, null, receipt, throttleRate)
        {
        }

        protected override PerceptionEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {
            return new PerceptionEventArgs(
                Symbol,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }
    }
}
