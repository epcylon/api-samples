namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Top Symbol values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    /// <remarks>
    /// Creates a new TopSymbolsEventArgs instance.
    /// </remarks>
    /// <param name="broker">The broker to get the Top Symbols for. Must match a valid broker type string.</param>
    /// <param name="instrumentType">The type of instrument to include in the results.</param>    
    /// <param name="symbols">Top symbol results.</param>
    /// <param name="error">Holds error information, if a subscription error occurred.</param>
    public class TopSymbolsEventArgs(string broker, InstrumentType instrumentType, 
                                     List<TopSymbol> symbols, SubscriptionError error = null) : 
        SubscriptionEventArgs(error)
    {
        /// <summary>
        /// Top symbol results.
        /// </summary>
        public IReadOnlyList<TopSymbol> Symbols { get; } = symbols;
        /// <summary>
        /// The broker to get the Top Symbols for. Must match a valid broker type string.
        /// </summary>
        public string Broker { get; } = broker;
        /// <summary>
        /// The type of instrument to include in the results.
        /// </summary>
        public InstrumentType InstrumentType { get; } = instrumentType;
    }
}
