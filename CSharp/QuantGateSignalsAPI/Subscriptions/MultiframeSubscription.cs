using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class MultiframeSubscription : GaugeSubscriptionBase<MultiframeUpdate, MultiframeEquilibrium>
    {
        public MultiframeSubscription(APIClient client, string streamID, string symbol,
                                      bool receipt = false, uint throttleRate = 0) :
               base(client, MultiframeUpdate.Parser, SubscriptionPath.GaugeMultiframeEquilibrium,
                    streamID, symbol, null, receipt, throttleRate)
        {
        }

        protected override MultiframeEquilibrium HandleUpdate(MultiframeUpdate update, object processed)
        {
            return new MultiframeEquilibrium
            {
                Symbol = Symbol,
                TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                Min5 = update.Min5 / 1000.0,
                Min10 = update.Min10 / 1000.0,
                Min15 = update.Min15 / 1000.0,
                Min30 = update.Min30 / 1000.0,
                Min45 = update.Min45 / 1000.0,
                Min60 = update.Min60 / 1000.0,
                Min120 = update.Min120 / 1000.0,
                Min180 = update.Min180 / 1000.0,
                Min240 = update.Min240 / 1000.0,
                Day1 = update.Day1 / 1000.0,
                IsDirty = update.IsDirty,
            };
        }
    }
}
