using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class EquilibriumSubscription : GaugeSubscriptionBase<EquilibriumUpdate, EquilibriumEventArgs>
    {
        public EquilibriumSubscription(APIClient client, string streamID, string symbol,
                                       string compression, bool receipt = false, uint throttleRate = 0) :
                base(client, EquilibriumUpdate.Parser, SubscriptionPath.GaugeEquilibrium,
                     streamID, symbol, compression, receipt, throttleRate)
        {
        }

        protected override EquilibriumEventArgs HandleUpdate(EquilibriumUpdate update, object processed)
        {
            return new EquilibriumEventArgs(
                Symbol, 
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp), 
                Compression,
                ProtoPriceEncoder.DecodePrice(update.EquilibriumPrice),
                ProtoPriceEncoder.DecodePrice(update.GapSize),
                ProtoPriceEncoder.DecodePrice(update.LastPrice),
                update.High / 1000.0,
                update.Low / 1000.0,
                update.Projected / 1000.0,
                update.Bias / 1000.0,
                update.IsDirty);
        }
    }
}
