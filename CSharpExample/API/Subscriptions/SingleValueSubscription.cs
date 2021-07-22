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
                                       string symbol, bool receipt = false, uint throttleRate = 0) :
                base(client, SingleValueUpdate.Parser, gaugePath, streamID, symbol, null, receipt, throttleRate)
        {
        }

        protected override void HandleUpdate(SingleValueUpdate update)
        {
            DateTime timestamp;

            timestamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);

            Values.TimeStamp = timestamp;
            Values.GaugeLevel = update.Value / 1000.0;
            Values.IsDirty = update.IsDirty;
        }
    }
}

