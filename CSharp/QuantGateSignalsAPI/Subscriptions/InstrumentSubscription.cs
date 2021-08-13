using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class InstrumentSubscription : SubscriptionBase<InstrumentUpdate, Instrument>
    {
        public InstrumentSubscription(APIClient client, string streamID, string symbol, 
                                      bool receipt = false, uint throttleRate = 0) :
            base(client, InstrumentUpdate.Parser,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnInstrument,
                                       ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol).Destination,
                 receipt, throttleRate)
        {
        }

        protected override void HandleUpdate(InstrumentUpdate update, object processed)
        {
            List<TickRange> tickRanges = new List<TickRange>();
            List<Values.TradingSession> tradingSessions = new List<Values.TradingSession>();
            Dictionary<string, string> brokerSymbols = new Dictionary<string, string>();

            Values.Symbol = update.Symbol;
            Values.Underlying = update.Underlying;
            Values.Currency = update.Currency;
            Values.Exchange = update.Exchange;
            Values.InstrumentType = (InstrumentType)update.InstrumentType;
            Values.PutOrCall = (PutOrCall)update.PutOrCall;
            Values.Strike = update.Strike;
            Values.ExpiryDate = ProtoTimeEncoder.DaysToDate(update.ExpiryDate);
            Values.Multiplier = update.Multiplier;
            Values.DisplayName = update.DisplayName;
            Values.TimeZone = TimeZoneDecoder.OlsonTimeZoneToTimeZoneInfo(update.TimeZone);

            foreach (TickValue range in update.TickValues)
            {
                tickRanges.Add(new TickRange
                {
                    Start = range.Start,
                    Tick = range.Tick,
                    Decimals = range.Decimals,
                    Denominator = (int)range.Denominator,
                    Format = (TickFormat)range.Format,
                });
            }
            Values.TickRanges = tickRanges;

            for (int day = 0; day < update.TradingSessions.Count; day++)
            {
                Proto.Stealth.TradingSession session = update.TradingSessions[day];
                tradingSessions.Add(new Values.TradingSession((System.DayOfWeek)day, session.Close, session.Length));
            }
            Values.TradingSessions = tradingSessions;

            foreach (KeyValuePair<string, string> symbolMapping in update.BrokerSymbols)
                brokerSymbols.Add(symbolMapping.Key, symbolMapping.Value);
            
            Values.BrokerSymbols = brokerSymbols;
        }
    }
}
