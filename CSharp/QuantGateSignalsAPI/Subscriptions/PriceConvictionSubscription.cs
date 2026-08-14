using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions;

internal class PriceConvictionSubscription(APIClient client, EventHandler<PriceConvictionEventArgs> handler,
                                           string streamID, string symbol, bool receipt = false,
                                           uint throttleRate = 0, object reference = null) :
    GaugeSubscriptionBase<PriceConvictionUpdate, PriceConvictionEventArgs>(
        client, PriceConvictionUpdate.Parser, handler, SubscriptionPath.GaugePriceConviction,
        ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, string.Empty, receipt, throttleRate, reference)
{
    protected override PriceConvictionEventArgs HandleUpdate(PriceConvictionUpdate update, object processed)
    {
        return new PriceConvictionEventArgs(
            Symbol, Stream,
            ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
            update.Conviction,
            update.IsDirty);
    }

    protected override PriceConvictionEventArgs WrapError(SubscriptionError error) =>
        new(Symbol, Stream, DateTime.UtcNow, 0, true, error);
}
