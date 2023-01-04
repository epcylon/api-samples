using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class PerceptionSubscription : GaugeSubscriptionBase<SingleValueUpdate, PerceptionEventArgs>
    {
        public PerceptionSubscription(APIClient client, EventHandler<PerceptionEventArgs> handler, 
                                      string streamID, string symbol, bool receipt = false,
                                      uint throttleRate = 0, object reference = null) :
            base(client, SingleValueUpdate.Parser, handler, SubscriptionPath.GaugePerception, 
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, 
                 string.Empty, receipt, throttleRate, reference)
        {
        }

        protected override PerceptionEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {            
            return new PerceptionEventArgs(
                Symbol, Stream,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }

        protected override PerceptionEventArgs WrapError(SubscriptionError error) =>
            new(Symbol, Stream, DateTime.UtcNow, 0, true, error);
    }
}
