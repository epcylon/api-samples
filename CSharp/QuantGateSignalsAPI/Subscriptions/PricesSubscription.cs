﻿using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.Utilities;

namespace QuantGate.API.Signals.Subscriptions;

internal class PricesSubscription(APIClient client, EventHandler<PricesEventArgs> handler, string streamID,
                                  string symbol, bool receipt = false, uint throttleRate = 0, object reference = null) :
    SubscriptionBase<QuoteUpdate, PricesEventArgs>(client, QuoteUpdate.Parser, handler,
         ParsedDestination.CreatePricesDestination(
                 ParsedDestination.StreamIDForSymbol(streamID, symbol), symbol).Destination, receipt, throttleRate, reference)
{
    private readonly PricesEventArgs _current = new(symbol, APIClient.ToStream(streamID));
    private readonly bool _isCrypto = symbol.Contains(':');

    protected override PricesEventArgs HandleUpdate(QuoteUpdate update, object processed)
    {
        _current.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);

        if (update.Realtime is not null)
        {
            _current.BidPrice = ProtoPriceEncoder.DecodePrice(update.Realtime.BidPrice);
            _current.BidSize = AdjustSize(update.Realtime.BidSize);
            _current.AskPrice = ProtoPriceEncoder.DecodePrice(update.Realtime.AskPrice);
            _current.AskSize = AdjustSize(update.Realtime.AskSize);
            _current.LastPrice = ProtoPriceEncoder.DecodePrice(update.Realtime.LastPrice);
            _current.LastSize = AdjustSize(update.Realtime.LastSize);
        }
        if (update.Snapshot is not null)
        {
            _current.Volume = AdjustSize(update.Snapshot.Volume);
            _current.OpenPrice = ProtoPriceEncoder.DecodePrice(update.Snapshot.OpenPrice);
            _current.HighPrice = ProtoPriceEncoder.DecodePrice(update.Snapshot.HighPrice);
            _current.LowPrice = ProtoPriceEncoder.DecodePrice(update.Snapshot.LowPrice);
            _current.ClosePrice = ProtoPriceEncoder.DecodePrice(update.Snapshot.ClosePrice);
            _current.PreviousClose = ProtoPriceEncoder.DecodePrice(update.Snapshot.PreviousClose);
            _current.High52Price = ProtoPriceEncoder.DecodePrice(update.Snapshot.High52Price);
            _current.Low52Price = ProtoPriceEncoder.DecodePrice(update.Snapshot.Low52Price);
        }
        if (update.Statistics is not null)
        {
            _current.Beta = update.Statistics.Beta;
            _current.Eps = update.Statistics.Eps;
            _current.PeRatio = update.Statistics.PeRatio;
            _current.SharesOutstanding = (long)update.Statistics.SharesOutstanding;
            _current.HistoricalVolatility = update.Statistics.HistoricalVolatility;
            _current.ImpliedVolatility = update.Statistics.ImpliedVolatility;
        }

        return (PricesEventArgs)_current.Clone();
    }

    private double AdjustSize(ulong size)
    {
        if (_isCrypto)
            return size / 100000000.0;
        else
            return size;
    }

    protected override PricesEventArgs WrapError(SubscriptionError error) =>
        new(_current.Symbol, _current.Stream, error) { TimeStamp = DateTime.UtcNow };
}
