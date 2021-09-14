using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class TriggerSubscription : GaugeSubscriptionBase<TriggerUpdate, TriggerEventArgs>
    {
        public TriggerSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, TriggerUpdate.Parser, SubscriptionPath.GaugeTrigger, streamID, symbol, null, receipt, throttleRate)
        {
        }

        protected override TriggerEventArgs HandleUpdate(TriggerUpdate update, object processed)
        {
            return new TriggerEventArgs(
                Symbol,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Perception / 1000.0,
                update.Commitment / 1000.0,
                update.Sentiment / 1000.0,
                ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice),
                ProtoPriceEncoder.DecodePrice(update.GapSize),
                ProtoPriceEncoder.DecodePrice(update.LastPrice),
                update.Bias / 1000.0,
                update.IsDirty
            );
        }
    }
}
