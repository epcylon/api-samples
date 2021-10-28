using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class PerceptionSubscription : GaugeSubscriptionBase<SingleValueUpdate, PerceptionEventArgs>
    {
        public PerceptionSubscription(APIClient client, EventHandler<PerceptionEventArgs> handler, 
                                      string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SingleValueUpdate.Parser, handler, SubscriptionPath.GaugePerception, 
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, string.Empty, receipt, throttleRate)
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
