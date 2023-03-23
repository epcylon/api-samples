using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class StrategySubscription : SubscriptionBase<StrategyUpdate, StrategyEventArgs>, ISymbolSubscription
    {
        private readonly string _symbol;
        private readonly DataStream _stream;
        private readonly string _strategyID;

        public StrategySubscription(APIClient client, EventHandler<StrategyEventArgs> handler,
                                    string strategyID, string streamID, string symbol,
                                    bool receipt = false, uint throttleRate = 0, object reference = null) :
            base(client, StrategyUpdate.Parser, handler,
                 new ParsedDestination(SubscriptionType.Strategy, SubscriptionPath.None,
                                       ParsedDestination.StreamIDForSymbol(streamID, symbol),
                                       symbol, strategyID: strategyID).Destination,
                 receipt, throttleRate, reference)
        {
            _symbol = symbol;
            _stream = APIClient.ToStream(streamID);
            _strategyID = strategyID;
        }

        string ISymbolSubscription.Symbol => _symbol;
        DataStream ISymbolSubscription.Stream => _stream;

        protected override StrategyEventArgs HandleUpdate(StrategyUpdate update, object processed)
        {
            return new StrategyEventArgs(
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                _symbol, _stream,
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
                (StrategySignal)update.Signal);
        }

        protected override StrategyEventArgs WrapError(SubscriptionError error) =>
            new(DateTime.UtcNow, _symbol, _stream, _strategyID, 0, 0, null,
                 0, null, 0, null, 0, null, 0, StrategySignal.None, error);

        /// <summary>
        /// Converts a level to a nullable double value.
        /// </summary>
        /// <param name="level">The level to convert.</param>
        /// <returns>The converted nullable double value.</returns>
        private static double? ConvertLevel(uint level) => level == 0 ? null : (double?)((int)level - 1001) / 1000.0;
    }
}
