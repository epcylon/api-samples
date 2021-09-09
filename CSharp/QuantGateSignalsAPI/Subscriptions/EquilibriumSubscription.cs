using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class EquilibriumSubscription : GaugeSubscriptionBase<EquilibriumUpdate, Equilibrium>
    {
        public EquilibriumSubscription(APIClient client, string streamID, string symbol,
                                       string compression, bool receipt = false, uint throttleRate = 0) :
               base(client, EquilibriumUpdate.Parser, SubscriptionPath.GaugeEquilibrium,
                    streamID, symbol, compression, receipt, throttleRate)
        {
        }

        protected override Equilibrium HandleUpdate(EquilibriumUpdate update, object processed)
        {
            return new Equilibrium
            {
                Symbol = Symbol,
                TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                EquilibriumPrice = ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice),
                GapSize = ProtoPriceEncoder.DecodePrice(update.GapSize),
                LastPrice = ProtoPriceEncoder.DecodePrice(update.LastPrice),
                High = update.High / 1000.0,
                Low = update.Low / 1000.0,
                Projected = update.Projected / 1000.0,
                Bias = update.Bias / 1000.0,
                IsDirty = update.IsDirty,
            };
        }
    }
}
