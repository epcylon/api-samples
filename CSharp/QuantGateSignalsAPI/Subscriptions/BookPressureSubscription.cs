﻿using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class BookPressureSubscription : GaugeSubscriptionBase<SingleValueUpdate, BookPressureEventArgs>
    {
        public BookPressureSubscription(APIClient client, EventHandler<BookPressureEventArgs> handler,
                                        string streamID, string symbol, bool receipt = false, 
                                        uint throttleRate = 0, object reference = null) :
            base(client, SingleValueUpdate.Parser, handler, SubscriptionPath.GaugeBookPressure, 
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, "0q", 
                 receipt, throttleRate, reference) { }

        protected override BookPressureEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {
            return new BookPressureEventArgs(
                Symbol, Stream,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }

        protected override BookPressureEventArgs WrapError(SubscriptionError error) => 
            new(Symbol, Stream, DateTime.UtcNow, 0, true, error);
    }
}
