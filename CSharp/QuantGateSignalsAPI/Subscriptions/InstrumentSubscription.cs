﻿using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class InstrumentSubscription : SubscriptionBase<InstrumentUpdate, InstrumentEventArgs>
    {
        /// <summary>
        /// Module-level identifier.
        /// </summary>
        private const string _moduleID = "InstSub";

        /// <summary>
        /// The symbol that was requested.
        /// </summary>
        private readonly string _symbol;
        private readonly DataStream _stream;

        public InstrumentSubscription(APIClient client, EventHandler<InstrumentEventArgs> handler,
                                      string streamID, string symbol, bool receipt = false,
                                      uint throttleRate = 0, object reference = null) :
            base(client, InstrumentUpdate.Parser, handler,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnInstrument,
                                       ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol).Destination,
                 receipt, throttleRate, reference)
        {
            _symbol = symbol;
            _stream = APIClient.ToStream(streamID);
        }

        protected override InstrumentEventArgs WrapError(SubscriptionError error) => new(_symbol, error);

        protected override InstrumentEventArgs HandleUpdate(InstrumentUpdate update, object processed)
        {
            List<TickRange> tickRanges = new();
            List<Events.TradingSession> tradingSessions = new();
            Dictionary<string, string> brokerSymbols = new();

            try
            {
                foreach (TickValue range in update.TickValues)
                {
                    tickRanges.Add(new TickRange(
                        range.Start,
                        range.Tick,
                        (int)range.Denominator,
                        range.Decimals,
                        (TickFormat)range.Format
                    ));
                }

                for (int day = 0; day < update.TradingSessions.Count; day++)
                {
                    Proto.Stealth.TradingSession session = update.TradingSessions[day];
                    tradingSessions.Add(new Events.TradingSession((DayOfWeek)day, session.Close, session.Length));
                }

                foreach (KeyValuePair<string, string> symbolMapping in update.BrokerSymbols)
                    brokerSymbols.Add(symbolMapping.Key, symbolMapping.Value);

                return new InstrumentEventArgs(_symbol, 
                    new Instrument(update.Symbol, update.Underlying, update.Currency, update.Exchange,
                                   (InstrumentType)update.InstrumentType, (PutOrCall)update.PutOrCall, 
                                   update.Strike, new DateTime((long)update.ExpiryDate, DateTimeKind.Utc),
                                   update.Multiplier, update.DisplayName,
                                   TimeZoneDecoder.OlsonTimeZoneToTimeZoneInfo(update.TimeZone),
                                   tickRanges, tradingSessions, brokerSymbols));
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":HUd - " + ex.Message);
                return new InstrumentEventArgs(_symbol, new SubscriptionError("Internal error handling update.", ex.Message));
            }
            finally
            {
                Unsubscribe();
            }
        }
    }
}
