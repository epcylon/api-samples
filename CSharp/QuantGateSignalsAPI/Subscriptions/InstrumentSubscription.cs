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
                tradingSessions.Add(new Values.TradingSession((System.DayOfWeek)day, session.Close, session.Length));
            }

            foreach (KeyValuePair<string, string> symbolMapping in update.BrokerSymbols)
                brokerSymbols.Add(symbolMapping.Key, symbolMapping.Value);

            return new InstrumentEventArgs(
                update.Symbol,
                update.Underlying,
                update.Currency,
                update.Exchange,
                (InstrumentType)update.InstrumentType,
                (PutOrCall)update.PutOrCall,
                update.Strike,
                ProtoTimeEncoder.DaysToDate(update.ExpiryDate),
                update.Multiplier,
                update.DisplayName,
                TimeZoneDecoder.OlsonTimeZoneToTimeZoneInfo(update.TimeZone),
                tickRanges,
                tradingSessions,
                brokerSymbols
            );
        }
    }
}
