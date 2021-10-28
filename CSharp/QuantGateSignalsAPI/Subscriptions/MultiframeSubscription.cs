﻿using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class MultiframeSubscription : GaugeSubscriptionBase<MultiframeUpdate, MultiframeEquilibriumEventArgs>
    {
        public MultiframeSubscription(APIClient client, EventHandler<MultiframeEquilibriumEventArgs> handler,
                                      string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
               base(client, MultiframeUpdate.Parser, handler, SubscriptionPath.GaugeMultiframeEquilibrium,
                    streamID, symbol, string.Empty, receipt, throttleRate)
        {
        }

        protected override MultiframeEquilibriumEventArgs HandleUpdate(MultiframeUpdate update, object processed)
        {
            return new MultiframeEquilibriumEventArgs(
                Symbol,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Min5 / 1000.0,
                update.Min10 / 1000.0,
                update.Min15 / 1000.0,
                update.Min30 / 1000.0,
                update.Min45 / 1000.0,
                update.Min60 / 1000.0,
                update.Min120 / 1000.0,
                update.Min180 / 1000.0,
                update.Min240 / 1000.0,
                update.Day1 / 1000.0,
                update.IsDirty
            );
        }
    }
}
