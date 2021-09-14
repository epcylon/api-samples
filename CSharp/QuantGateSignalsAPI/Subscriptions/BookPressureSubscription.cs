using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class BookPressureSubscription : GaugeSubscriptionBase<SingleValueUpdate, BookPressureEventArgs>
    {
        public BookPressureSubscription(APIClient client, string streamID, string symbol, 
                                        bool receipt = false, uint throttleRate = 0) :
            base(client, SingleValueUpdate.Parser, SubscriptionPath.GaugeBookPressure, 
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol, "0q", receipt, throttleRate) { }

        protected override BookPressureEventArgs HandleUpdate(SingleValueUpdate update, object processed)
        {
            return new BookPressureEventArgs(
                Symbol,
                ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp),
                update.Value / 1000.0,
                update.IsDirty);
        }
    }
}
