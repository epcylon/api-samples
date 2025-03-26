namespace QuantGate.API.Signals.Events;

/// <summary>
/// Creates a new PricesEventArgs instance.
/// </summary>
/// <param name="symbol">The symbol we are getting prices for.</param>
/// <param name="error">Holds error information, if a subscription error occurred.</param>
public class PricesEventArgs(string symbol, DataStream stream, SubscriptionError error = null) :
    SubscriptionEventArgs(error)
{
    /// <summary>
    /// The symbol we are getting prices for.
    /// </summary>
    public string Symbol { get; } = symbol;
    public DataStream Stream { get; } = stream;

    /// <summary>
    /// Timestamp of the latest update.
    /// </summary>
    public DateTime TimeStamp { get; protected internal set; }

    /// <summary>
    /// The inside bid price.
    /// </summary>
    public double BidPrice { get; protected internal set; }
    /// <summary>
    /// The inside bid size.
    /// </summary>
    public double BidSize { get; protected internal set; }
    /// <summary>
    /// The inside ask price.
    /// </summary>
    public double AskPrice { get; protected internal set; }
    /// <summary>
    /// The inside ask size.
    /// </summary>
    public double AskSize { get; protected internal set; }
    /// <summary>
    /// The inside last price.
    /// </summary>
    public double LastPrice { get; protected internal set; }
    /// <summary>
    /// The inside last size.
    /// </summary>
    public double LastSize { get; protected internal set; }
    public double Volume { get; protected internal set; }
    public double OpenPrice { get; protected internal set; }
    public double HighPrice { get; protected internal set; }
    public double LowPrice { get; protected internal set; }
    public double ClosePrice { get; protected internal set; }
    public double PreviousClose { get; protected internal set; }
    public double High52Price { get; protected internal set; }
    public double Low52Price { get; protected internal set; }
    public double Beta { get; protected internal set; }
    public double Eps { get; protected internal set; }
    public double PeRatio { get; protected internal set; }
    public long SharesOutstanding { get; protected internal set; }
    public double HistoricalVolatility { get; protected internal set; }
    public double ImpliedVolatility { get; protected internal set; }

    /// <summary>
    /// Creates a cloned copy of this object.
    /// </summary>
    /// <returns>A cloned copy of this object.</returns>
    protected internal override object Clone() => new PricesEventArgs(Symbol, Stream, Error)
    {
        TimeStamp = TimeStamp,
        BidPrice = BidPrice,
        BidSize = BidSize,
        AskPrice = AskPrice,
        AskSize = AskSize,
        LastPrice = LastPrice,
        LastSize = LastSize,
        Volume = Volume,
        OpenPrice = OpenPrice,
        HighPrice = HighPrice,
        LowPrice = LowPrice,
        ClosePrice = ClosePrice,
        PreviousClose = PreviousClose,
        High52Price = High52Price,
        Low52Price = Low52Price,
        Beta = Beta,
        Eps = Eps,
        PeRatio = PeRatio,
        SharesOutstanding = SharesOutstanding,
        HistoricalVolatility = HistoricalVolatility,
        ImpliedVolatility = ImpliedVolatility,
        Reference = Reference,
    };
}
