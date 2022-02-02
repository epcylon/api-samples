using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class TopSymbolsSubscription : SubscriptionBase<TopSymbolsUpdate, TopSymbolsEventArgs>
    {
        /// <summary>
        /// The broker to get the Top Symbols for. Must match a valid broker type string.
        /// </summary>
        public string Broker { get; }
        /// <summary>
        /// The type of instrument to include in the results.
        /// </summary>
        public InstrumentType InstrumentType { get; }
        /// <summary>
        /// Reference object tied to the initial request.
        /// </summary>
        public object Reference { get; }

        public TopSymbolsSubscription(APIClient client, EventHandler<TopSymbolsEventArgs> handler, string broker,
                                      InstrumentType instrumentType = InstrumentType.NoInstrument,
                                      bool receipt = false, uint throttleRate = 0, object reference = null) :
            base(client, TopSymbolsUpdate.Parser, handler,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnTopSymbols, string.Empty,
                                       broker: broker, securityType: InstrumentTypeToString(instrumentType)).Destination,
                 receipt, throttleRate)
        {
            Broker = broker;
            InstrumentType = instrumentType;
            Reference = reference;            
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
                default: return string.Empty;
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
            return new TopSymbolsEventArgs(Broker, InstrumentType, (List<TopSymbol>)processed, Reference);
        }

        protected override TopSymbolsEventArgs WrapError(SubscriptionError error) =>
            new TopSymbolsEventArgs(Broker, InstrumentType, new List<TopSymbol>(), Reference, error);
    }
}
