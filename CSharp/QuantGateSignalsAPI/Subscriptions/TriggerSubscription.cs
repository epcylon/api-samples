using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class TriggerSubscription : GaugeSubscriptionBase<TriggerUpdate, TriggerEventArgs>
    {
        public TriggerSubscription(APIClient client, EventHandler<TriggerEventArgs> handler, string streamID, 
                                   string symbol, bool receipt = false, uint throttleRate = 0, object reference = null) :
            base(client, TriggerUpdate.Parser, handler, SubscriptionPath.GaugeTrigger, 
                 streamID, symbol, string.Empty, receipt, throttleRate, reference)
        {
        }

        protected override TriggerEventArgs HandleUpdate(TriggerUpdate update, object processed)
        {
            return new TriggerEventArgs(
                Symbol, Stream,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Perception / 1000.0,
                update.Commitment / 1000.0,
                update.Sentiment / 1000.0,
                ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice),
                ProtoPriceEncoder.DecodePrice(update.GapSize),
                ProtoPriceEncoder.DecodePrice(update.LastPrice),
                update.Bias / 1000.0,
                update.IsDirty);
        }

        protected override TriggerEventArgs WrapError(SubscriptionError error) =>
            new(Symbol, Stream, DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, true, error);
    }
}
