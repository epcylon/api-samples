using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class BookPressureSubscription : SingleValueSubscription<BookPressure>
    {
        public BookPressureSubscription(ProtoStompClient client, string streamID, string symbol, bool receipt = false, uint throttleRate = 0) :
            base(client, SubscriptionPath.GaugeBookPressure, streamID, symbol, receipt, throttleRate)
        {
        }
    }
}
