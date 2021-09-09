namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// The details of an individual symbol in response to a symbol search request.
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// Symbol as listed by the QuantGate servers.
        /// </summary>
        public string Symbol { get; }
        /// <summary>
        /// Underlying symbol.
        /// </summary>
        public string Underlying { get; }
        /// <summary>
        /// Currency the instrument is traded in.
        /// </summary>
        public string Currency { get; }
        /// <summary>
        /// Type of instrument.
        /// </summary>
        public InstrumentType InstrumentType { get; }
        /// <summary>
        /// Primary exchange (ISO 10383 MIC) the instrument is traded on.
        /// </summary>
        public string Exchange { get; }
        /// <summary>        
        /// Display name of the instrument.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Creates a new SearchResult instance.
        /// </summary>
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="underlying">Underlying symbol.</param>
        /// <param name="currency">Currency the instrument is traded in.</param>
        /// <param name="instrumentType">Type of instrument.</param>
        /// <param name="exchange">Primary exchange (ISO 10383 MIC) the instrument is traded on.</param>
        /// <param name="displayName">Display name of the instrument.</param>
        internal SearchResult(string symbol, string underlying, string currency,
                              InstrumentType instrumentType, string exchange, string displayName)
        {
            Symbol = symbol;
            Underlying = underlying;
            Currency = currency;
            InstrumentType = instrumentType;
            Exchange = exchange;
            DisplayName = displayName;
        }
    }
}
