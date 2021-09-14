using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class TopSymbolsSubscription : SubscriptionBase<TopSymbolsUpdate, TopSymbolsEventArgs>
    {
        public TopSymbolsSubscription(APIClient client, string streamID, string broker,
                                      InstrumentType instrumentType = InstrumentType.NoInstrument,
                                      bool receipt = false, uint throttleRate = 0) :
            base(client, TopSymbolsUpdate.Parser,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnTopSymbols, streamID,
                                       broker: broker, securityType: InstrumentTypeToString(instrumentType)).Destination,
                 receipt, throttleRate)
        {
        }

        internal static string InstrumentTypeToString(InstrumentType type)
        {
            switch (type)
            {
                case InstrumentType.CommonStock: return "CS";
                case InstrumentType.Future: return "FUT";
                case InstrumentType.ForexContract: return "FX";
                case InstrumentType.CryptoCurrency: return "CRY";
                case InstrumentType.Index: return "IDX";
                default: return null;
            }
        }

        protected override object Preprocess(TopSymbolsUpdate update)
        {
            List<TopSymbol> results = new List<TopSymbol>();

            foreach (TopSymbolItem result in update.Symbols)
                results.Add(new TopSymbol(
                    ProtoTimeEncoder.TimestampSecondsToDate(result.Timestamp),
                    result.Symbol,
                    result.Underlying,
                    result.Currency,
                    (InstrumentType)result.InstrumentType,
                    result.Exchange,
                    result.DisplayName,
                    result.EntryProgress / 1000.0,
                    (GaugeSignal)result.PerceptionSignal,
                    (GaugeSignal)result.CommitmentSignal,
                    (GaugeSignal)result.EquilibriumSignal,
                    (GaugeSignal)result.SentimentSignal,
                    (StrategySignal)result.Signal));

            return results;
        }

        protected override TopSymbolsEventArgs HandleUpdate(TopSymbolsUpdate update, object processed)
        {
            return new TopSymbolsEventArgs(processed as List<TopSymbol>);
        }
    }
}
