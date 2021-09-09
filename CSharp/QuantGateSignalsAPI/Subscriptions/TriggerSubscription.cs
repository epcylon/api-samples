using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

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
            return new TriggerEventArgs
            {
                Symbol = Symbol,
                TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                Bias = update.Bias / 1000.0,
                Perception = update.Perception / 1000.0,
                Sentiment = update.Sentiment / 1000.0,
                Commitment = update.Commitment / 1000.0,
                EquilibriumPrice = ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice),
                GapSize = ProtoPriceEncoder.DecodePrice(update.GapSize),
                LastPrice = ProtoPriceEncoder.DecodePrice(update.LastPrice),
                IsDirty = update.IsDirty,
            };
        }
    }
}
