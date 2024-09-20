using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class TopSymbolsSubscription(APIClient client, EventHandler<TopSymbolsEventArgs> handler, string broker,
                                          InstrumentType instrumentType = InstrumentType.NoInstrument,
                                          bool receipt = false, uint throttleRate = 0, object reference = null) : 
        SubscriptionBase<TopSymbolsUpdate, TopSymbolsEventArgs>(client, TopSymbolsUpdate.Parser, handler,
             new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnTopSymbols, string.Empty,
                                   broker: broker, securityType: InstrumentTypeToString(instrumentType)).Destination,
                                   receipt, throttleRate, reference)
    {
        /// <summary>
        /// The broker to get the Top Symbols for. Must match a valid broker type string.
        /// </summary>
        public string Broker { get; } = broker;
        /// <summary>
        /// The type of instrument to include in the results.
        /// </summary>
        public InstrumentType InstrumentType { get; } = instrumentType;

        internal static string InstrumentTypeToString(InstrumentType type)
        {
            return type switch
            {
                InstrumentType.CommonStock => "CS",
                InstrumentType.Future => "FUT",
                InstrumentType.ForexContract => "FX",
                InstrumentType.CryptoCurrency => "CRY",
                InstrumentType.Index => "IDX",
                _ => string.Empty,
            };
        }

        protected override object Preprocess(TopSymbolsUpdate update)
        {
            List<TopSymbol> results = new();

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
            return new TopSymbolsEventArgs(Broker, InstrumentType, (List<TopSymbol>)processed);
        }

        protected override TopSymbolsEventArgs WrapError(SubscriptionError error) =>
            new(Broker, InstrumentType, new List<TopSymbol>(), error);
    }
}
