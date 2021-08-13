using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class StrategySubscription : SubscriptionBase<StrategyUpdate, StrategyValues>
    {
        public StrategySubscription(APIClient client, string strategyID, string streamID,
                                    string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, StrategyUpdate.Parser,
                 new ParsedDestination(SubscriptionType.Strategy, SubscriptionPath.None,
                                       ParsedDestination.StreamIDForSymbol(streamID, symbol),
                                       symbol, strategyID: strategyID).Destination,
                 receipt, throttleRate)
        {
        }

        protected override void HandleUpdate(StrategyUpdate update, object processed)
        {
            Values.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);
            Values.EntryProgress = update.EntryProgress / 1000.0;
            Values.ExitProgress = update.ExitProgress / 1000.0;
            Values.Signal = (StrategySignal)update.Signal;
            Values.PerceptionLevel = ConvertLevel(update.PerceptionLevel);
            Values.PerceptionSignal = (GaugeSignal)update.PerceptionSignal;
            Values.CommitmentLevel = ConvertLevel(update.CommitmentLevel);
            Values.CommitmentSignal = (GaugeSignal)update.CommitmentSignal;
            Values.EquilibriumLevel = ConvertLevel(update.EquilibriumLevel);
            Values.EquilibriumSignal = (GaugeSignal)update.EquilibriumSignal;
            Values.SentimentLevel = ConvertLevel(update.SentimentLevel);
            Values.SentimentSignal = (GaugeSignal)update.SentimentSignal;
        }

        /// <summary>
        /// Converts a level to a nullable double value.
        /// </summary>
        /// <param name="level">The level to convert.</param>
        /// <returns>The converted nullable double value.</returns>
        private double? ConvertLevel(uint level) => level == 0 ? null : (double?)((level - 1001) / 1000.0);
    }
}
