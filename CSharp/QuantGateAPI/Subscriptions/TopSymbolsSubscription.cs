using QuantGate.API.Proto.Stealth;
using QuantGate.API.Utilities;
using QuantGate.API.Values;
using System.Collections.Generic;

namespace QuantGate.API.Subscriptions
{
    internal class TopSymbolsSubscription : SubscriptionBase<TopSymbolsUpdate, TopSymbols>
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

        private static string InstrumentTypeToString(InstrumentType type)
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
                results.Add(new TopSymbol
                {
                    Timestamp = ProtoTimeEncoder.TimestampSecondsToDate(result.Timestamp),
                    Symbol = result.Symbol,
                    Underlying = result.Underlying,
                    Currency = result.Currency,
                    DisplayName = result.DisplayName,
                    Exchange = result.Exchange,
                    InstrumentType = (InstrumentType)result.InstrumentType,
                    Signal = (StrategySignal)result.Signal,
                    EntryProgress = result.EntryProgress / 1000.0,
                    PerceptionSignal = (GaugeSignal)result.PerceptionSignal,
                    CommitmentSignal = (GaugeSignal)result.CommitmentSignal,
                    EquilibriumSignal = (GaugeSignal)result.EquilibriumSignal,
                    SentimentSignal = (GaugeSignal)result.SentimentSignal
                }); ;

            return results;
        }

        protected override void HandleUpdate(TopSymbolsUpdate update, object processed)
        {
            Values.Symbols = processed as List<TopSymbol>;
        }
    }
}
