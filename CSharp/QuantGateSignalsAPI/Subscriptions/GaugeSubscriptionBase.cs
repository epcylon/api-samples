using Google.Protobuf;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class GaugeSubscriptionBase<M, V> : SubscriptionBase<M, V>, ISymbolSubscription
        where M : IMessage<M>
        where V : GaugeEventArgs
    {
        public string Symbol { get; }
        public DataStream Stream { get; }
        internal string Compression { get; }

        public GaugeSubscriptionBase(APIClient client, MessageParser<M> parser, EventHandler<V> handler,
                                     SubscriptionPath gaugePath, string streamID, string symbol,
                                     string compression = "", bool receipt = false,
                                     uint throttleRate = 0, object reference = null) :
            base(client, parser, handler, ParsedDestination.CreateGaugeDestination
                    (gaugePath, streamID, symbol, compression).Destination, receipt, throttleRate, reference)
        {
            Symbol = symbol;
            Stream = APIClient.ToStream(streamID);
            Compression = compression;
        }
    }
}
