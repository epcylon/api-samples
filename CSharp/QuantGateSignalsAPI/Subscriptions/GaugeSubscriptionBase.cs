using Google.Protobuf;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Events;
using System;

namespace QuantGate.API.Signals.Subscriptions
{
    internal abstract class GaugeSubscriptionBase<M, V> : SubscriptionBase<M, V>, ISymbolSubscription
        where M : IMessage<M>
        where V : GaugeArgsBase
    {
        public string Symbol { get; }
        internal string Compression { get; }

        public GaugeSubscriptionBase(APIClient client, MessageParser<M> parser, EventHandler<V> handler,
                                     SubscriptionPath gaugePath, string streamID, string symbol, 
                                     string compression = "", bool receipt = false, uint throttleRate = 0) :
            base(client, parser, handler, ParsedDestination.CreateGaugeDestination
                    (gaugePath, streamID, symbol, compression).Destination, receipt, throttleRate)
        {
            Symbol = symbol;
            Compression = compression;
        }
    }
}
