namespace QuantGate.API.Signals.Subscriptions;

/// <summary>
/// Interface for a subscription tied to a symbol.
/// </summary>
internal interface ISymbolSubscription
{
    /// <summary>
    /// The symbol the subscription is tied to.
    /// </summary>
    string Symbol { get; }
    DataStream Stream { get; }
}
