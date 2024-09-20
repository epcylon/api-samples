using Google.Protobuf;
using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class GaugeSubscriptionBase<M, V>(
            APIClient client, MessageParser<M> parser, EventHandler<V> handler, SubscriptionPath gaugePath, string streamID, 
            string symbol, string compression = "", bool receipt = false, uint throttleRate = 0, object reference = null) : 
        SubscriptionBase<M, V>(client, parser, handler, 
                               ParsedDestination.CreateGaugeDestination(gaugePath, streamID, symbol, compression).Destination,
                               receipt, throttleRate, reference), 
        ISymbolSubscription
        where M : IMessage<M>
        where V : GaugeEventArgs
    {
        public string Symbol { get; } = symbol;
        public DataStream Stream { get; } = APIClient.ToStream(streamID);
        internal string Compression { get; } = compression;
    }
}
