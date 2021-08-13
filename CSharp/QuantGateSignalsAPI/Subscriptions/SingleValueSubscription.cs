using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class SingleValueSubscription<V> : GaugeSubscriptionBase<SingleValueUpdate, V>
        where V : SingleValueBase, new()
    {
        public SingleValueSubscription(APIClient client, SubscriptionPath gaugePath, string streamID,
                                       string symbol, string compression = null, bool receipt = false, uint throttleRate = 0) :
                base(client, SingleValueUpdate.Parser, gaugePath, ParsedDestination.StreamIDForSymbol(streamID, symbol), 
                     symbol, compression, receipt, throttleRate)
        {
        }

        protected override void HandleUpdate(SingleValueUpdate update, object processed)
        {
            Values.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);
            Values.GaugeLevel = update.Value / 1000.0;
            Values.IsDirty = update.IsDirty;
        }
    }
}

