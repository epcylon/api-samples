using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using QuantGate.API.Proto.Stealth;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class TriggerSubscription : GaugeSubscriptionBase<TriggerUpdate, Trigger>
    {
        public TriggerSubscription(ProtoStompClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
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
