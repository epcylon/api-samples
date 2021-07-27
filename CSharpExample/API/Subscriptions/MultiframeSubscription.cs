using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using QuantGate.API.Proto.Stealth;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class MultiframeSubscription : GaugeSubscriptionBase<MultiframeUpdate, MultiFrameEquilibrium>
    {
        public MultiframeSubscription(ProtoStompClient client, string streamID, string symbol,
                                     string compression, bool receipt = false, uint throttleRate = 0) :
               base(client, MultiframeUpdate.Parser, SubscriptionPath.GaugeMultiframeEquilibrium,
                    streamID, symbol, compression, receipt, throttleRate)
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
