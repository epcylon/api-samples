using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class InstrumentSubscription : SubscriptionBase<InstrumentUpdate, InstrumentEventArgs>
    {
        public InstrumentSubscription(APIClient client, string streamID, string symbol,
                                      bool receipt = false, uint throttleRate = 0) :
            base(client, InstrumentUpdate.Parser,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnInstrument,
                                       ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol).Destination,
                 receipt, throttleRate)
        {
        }

        protected override InstrumentEventArgs HandleUpdate(InstrumentUpdate update, object processed)
        {
            List<TickRange> tickRanges = new List<TickRange>();
            List<Values.TradingSession> tradingSessions = new List<Values.TradingSession>();
            Dictionary<string, string> brokerSymbols = new Dictionary<string, string>();

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

            for (int day = 0; day < update.TradingSessions.Count; day++)
            {
                Proto.Stealth.TradingSession session = update.TradingSessions[day];
                tradingSessions.Add(new Values.TradingSession((System.DayOfWeek)day, session.Close, session.Length));
            }

            foreach (KeyValuePair<string, string> symbolMapping in update.BrokerSymbols)
                brokerSymbols.Add(symbolMapping.Key, symbolMapping.Value);

            return new InstrumentEventArgs
            {
                Symbol = update.Symbol,
                Underlying = update.Underlying,
                Currency = update.Currency,
                Exchange = update.Exchange,
                InstrumentType = (InstrumentType)update.InstrumentType,
                PutOrCall = (PutOrCall)update.PutOrCall,
                Strike = update.Strike,
                ExpiryDate = ProtoTimeEncoder.DaysToDate(update.ExpiryDate),
                Multiplier = update.Multiplier,
                DisplayName = update.DisplayName,
                TimeZone = TimeZoneDecoder.OlsonTimeZoneToTimeZoneInfo(update.TimeZone),
                TickRanges = tickRanges,
                TradingSessions = tradingSessions,
                BrokerSymbols = brokerSymbols,
            };
        }
    }
}
