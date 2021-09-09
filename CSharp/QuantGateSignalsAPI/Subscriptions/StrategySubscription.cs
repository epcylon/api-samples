using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class StrategySubscription : SubscriptionBase<StrategyUpdate, StrategyEventArgs>
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

        protected override StrategyEventArgs HandleUpdate(StrategyUpdate update, object processed)
        {
            return new StrategyEventArgs
            {
                Symbol = _symbol,
                StrategyID = _strategyID,
                TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                EntryProgress = update.EntryProgress / 1000.0,
                ExitProgress = update.ExitProgress / 1000.0,
                Signal = (StrategySignal)update.Signal,
                PerceptionLevel = ConvertLevel(update.PerceptionLevel),
                PerceptionSignal = (GaugeSignal)update.PerceptionSignal,
                CommitmentLevel = ConvertLevel(update.CommitmentLevel),
                CommitmentSignal = (GaugeSignal)update.CommitmentSignal,
                EquilibriumLevel = ConvertLevel(update.EquilibriumLevel),
                EquilibriumSignal = (GaugeSignal)update.EquilibriumSignal,
                SentimentLevel = ConvertLevel(update.SentimentLevel),
                SentimentSignal = (GaugeSignal)update.SentimentSignal,
            };
        }

        /// <summary>
        /// Converts a level to a nullable double value.
        /// </summary>
        /// <param name="level">The level to convert.</param>
        /// <returns>The converted nullable double value.</returns>
        private double? ConvertLevel(uint level) => level == 0 ? null : (double?)((level - 1001) / 1000.0);
    }
}
