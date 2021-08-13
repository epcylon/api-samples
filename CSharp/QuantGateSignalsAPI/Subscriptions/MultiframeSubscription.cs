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

        protected override void HandleUpdate(MultiframeUpdate update, object processed)
        {
            Values.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);
            Values.Min5 = update.Min5 / 1000.0;
            Values.Min10 = update.Min10 / 1000.0;
            Values.Min15 = update.Min15 / 1000.0;
            Values.Min30 = update.Min30 / 1000.0;
            Values.Min45 = update.Min45 / 1000.0;
            Values.Min60 = update.Min60 / 1000.0;
            Values.Min120 = update.Min120 / 1000.0;
            Values.Min180 = update.Min180 / 1000.0;
            Values.Min240 = update.Min240 / 1000.0;
            Values.Day1 = update.Day1 / 1000.0;
            Values.IsDirty = update.IsDirty;
        }
    }
}
