using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class BookPressureSubscription : SingleValueSubscription<BookPressure>
    {
        public BookPressureSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeBookPressure, streamID, symbol, "0q", receipt, throttleRate)
        {
        }
    }
}
