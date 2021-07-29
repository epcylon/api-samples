using QuantGate.API.Utilities;
using QuantGate.API.Values;

namespace QuantGate.API.Subscriptions
{
    internal class BookPressureSubscription : SingleValueSubscription<BookPressure>
    {
        public BookPressureSubscription(APIClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeBookPressure, streamID, symbol, "0q", receipt, throttleRate)
        {
        }
    }
}
