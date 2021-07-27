using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using QuantGate.API.Proto.Stealth;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class EquilibriumSubscription : GaugeSubscriptionBase<EquilibriumUpdate, Equilibrium>
    {
        public EquilibriumSubscription(ProtoStompClient client, string streamID, string symbol,
                                       string compression, bool receipt = false, uint throttleRate = 0) :
               base(client, EquilibriumUpdate.Parser, SubscriptionPath.GaugeEquilibrium,
                    streamID, symbol, compression, receipt, throttleRate)
        {
        }

        protected override void HandleUpdate(EquilibriumUpdate update, object processed)
        {
            Values.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);
            Values.EquilibriumPrice = ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice);
            Values.GapSize = ProtoPriceEncoder.DecodePrice(update.GapSize);
            Values.LastPrice = ProtoPriceEncoder.DecodePrice(update.LastPrice);
            Values.High = update.High / 1000.0;
            Values.Low = update.Low / 1000.0;
            Values.Projected = update.Projected / 1000.0;
            Values.Bias = update.Bias / 1000.0;
            Values.IsDirty = update.IsDirty;
        }
    }
}
