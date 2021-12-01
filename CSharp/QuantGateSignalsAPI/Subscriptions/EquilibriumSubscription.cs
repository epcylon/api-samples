﻿using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class EquilibriumSubscription : GaugeSubscriptionBase<EquilibriumUpdate, EquilibriumEventArgs>
    {
        public EquilibriumSubscription(APIClient client, EventHandler<EquilibriumEventArgs> handler, string streamID, 
                                       string symbol, string compression, bool receipt = false, uint throttleRate = 0) :
                base(client, EquilibriumUpdate.Parser, handler, SubscriptionPath.GaugeEquilibrium,
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

        protected override EquilibriumEventArgs WrapError(SubscriptionError error)
            => new EquilibriumEventArgs(Symbol, DateTime.UtcNow, Compression, 0, 0, 0, 0, 0, 0, 0, true, error);
    }
}
