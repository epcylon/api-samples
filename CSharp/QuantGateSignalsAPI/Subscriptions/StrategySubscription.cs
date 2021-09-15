﻿using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class StrategySubscription : SubscriptionBase<StrategyUpdate, StrategyEventArgs>, ISymbolSubscription
    {
        private string _symbol;
        private string _strategyID;

        public StrategySubscription(APIClient client, string strategyID, string streamID,
                                    string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, StrategyUpdate.Parser,
                 new ParsedDestination(SubscriptionType.Strategy, SubscriptionPath.None,
                                       ParsedDestination.StreamIDForSymbol(streamID, symbol),
                                       symbol, strategyID: strategyID).Destination,
                 receipt, throttleRate)
        {
            _symbol = symbol;
            _strategyID = strategyID;
        }

        string ISymbolSubscription.Symbol => _symbol;

        protected override StrategyEventArgs HandleUpdate(StrategyUpdate update, object processed)
        {
            return new StrategyEventArgs(
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                _symbol,
                _strategyID,
                update.EntryProgress / 1000.0,
                update.ExitProgress / 1000.0,
                ConvertLevel(update.PerceptionLevel),
                (GaugeSignal)update.PerceptionSignal,
                ConvertLevel(update.CommitmentLevel),
                (GaugeSignal)update.CommitmentSignal,
                ConvertLevel(update.EquilibriumLevel),
                (GaugeSignal)update.EquilibriumSignal,
                ConvertLevel(update.SentimentLevel),
                (GaugeSignal)update.SentimentSignal,
                (StrategySignal)update.Signal
            );
        }

        /// <summary>
        /// Converts a level to a nullable double value.
        /// </summary>
        /// <param name="level">The level to convert.</param>
        /// <returns>The converted nullable double value.</returns>
        private double? ConvertLevel(uint level) => level == 0 ? null : (double?)((level - 1001) / 1000.0);
    }
}
