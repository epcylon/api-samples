using QuantGate.API.Proto.Stealth;
using QuantGate.API.Utilities;
using QuantGate.API.Values;

namespace QuantGate.API.Subscriptions
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
            Values.PerceptionLevel = update.PerceptionLevel / 1000.0;
            Values.PerceptionSignal = (GaugeSignal)update.PerceptionSignal;
            Values.CommitmentLevel = update.CommitmentLevel / 1000.0;
            Values.CommitmentSignal = (GaugeSignal)update.CommitmentSignal;
            Values.EquilibriumLevel = update.EquilibriumLevel / 1000.0;
            Values.EquilibriumSignal = (GaugeSignal)update.EquilibriumSignal;
            Values.SentimentLevel = update.SentimentLevel / 1000.0;
            Values.SentimentSignal = (GaugeSignal)update.SentimentSignal;
        }
    }
}
