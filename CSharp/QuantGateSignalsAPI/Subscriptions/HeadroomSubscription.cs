using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class HeadroomSubscription : GaugeSubscriptionBase<SingleValueUpdate, HeadroomEventArgs>
    {
        public HeadroomSubscription(APIClient client, EventHandler<HeadroomEventArgs> handler, string streamID,
                                    string symbol, bool receipt = false, uint throttleRate = 0, object reference = null) :
            base(client, SingleValueUpdate.Parser, handler, SubscriptionPath.GaugeHeadroom,
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, "5m", receipt, throttleRate, reference)
        {
        }

        protected override HeadroomEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {
            return new HeadroomEventArgs(
                Symbol, Stream,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }

        protected override HeadroomEventArgs WrapError(SubscriptionError error) =>
            new(Symbol, Stream, DateTime.UtcNow, 0, true, error);
    }
}
