using QuantGate.API.Proto.Stealth;
using QuantGate.API.Utilities;
using QuantGate.API.Values;

namespace QuantGate.API.Subscriptions
{
    internal class TriggerSubscription : GaugeSubscriptionBase<TriggerUpdate, Trigger>
    {
        public TriggerSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, TriggerUpdate.Parser, SubscriptionPath.GaugeTrigger, streamID, symbol, null, receipt, throttleRate)
        {
        }

        protected override void HandleUpdate(TriggerUpdate update, object processed)
        {
            Values.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);
            Values.Bias = update.Bias / 1000.0;
            Values.PerceptionPrice = update.Perception / 1000.0;
            Values.SentimentPrice = update.Sentiment / 1000.0;
            Values.CommitmentPrice = update.Commitment / 1000.0;
            Values.EquilibriumPrice = ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice);
            Values.GapSize = ProtoPriceEncoder.DecodePrice(update.GapSize);
            Values.LastPrice = ProtoPriceEncoder.DecodePrice(update.LastPrice);
            Values.IsDirty = update.IsDirty;
        }
    }
}
