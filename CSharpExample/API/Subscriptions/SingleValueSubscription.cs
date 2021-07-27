using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using QuantGate.API.Proto.Stealth;
using System;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public abstract class SingleValueSubscription<V> : GaugeSubscriptionBase<SingleValueUpdate, V>
        where V : SingleValueBase, new()
    {
        public SingleValueSubscription(ProtoStompClient client, SubscriptionPath gaugePath, string streamID,
                                       string symbol, string compression = null, bool receipt = false, uint throttleRate = 0) :
                base(client, SingleValueUpdate.Parser, gaugePath, streamID, symbol, compression, receipt, throttleRate)
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

