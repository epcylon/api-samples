namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Basic details about an instrument (not including trading day information, etc.)
    /// </summary>
    public class InstrumentBase
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
        /// Primary exchange (ISO 10383 MIC) the instrument is traded on.
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Display name of the instrument.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Type of instrument. 
        /// </summary>
        public InstrumentType InstrumentType { get; }

        /// <summary>
        /// Right of an option, if an option (will be empty otherwise).
        /// </summary>
        public PutOrCall PutOrCall { get; }

        /// <summary>
        /// Strike price of an option, if an option (will be zero otherwise).
        /// </summary>
        public double Strike { get; }

        /// <summary>
        /// Expiry date of the instrument, if applicable.
        /// </summary>
        public DateTime ExpiryDate { get; }

        /// <summary>
        /// Price multiplier (to convert price to value).
        /// </summary>
        public double Multiplier { get; }

        /// <summary>
        /// Creates a new InstrumentEventArgs instance.
        /// </summary>
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="underlying">Underlying symbol.</param>
        /// <param name="currency">Currency the instrument is traded in.</param>
        /// <param name="exchange">Primary exchange (ISO 10383 MIC) the instrument is traded on.</param>
        /// <param name="displayName">Display name of the instrument.</param>
        /// <param name="instrumentType">Type of instrument.</param>
        /// <param name="putOrCall">Right of an option, if an option (will be empty otherwise).</param>
        /// <param name="strike">Strike price of an option, if an option (will be zero otherwise).</param>
        /// <param name="expiryDate">Expiry date of the instrument, if applicable.</param>
        /// <param name="multiplier">Price multiplier (to convert price to value).</param>
        internal InstrumentBase(string symbol, string underlying, string currency, string exchange,
                                InstrumentType instrumentType, PutOrCall putOrCall, double strike,
                                DateTime expiryDate, double multiplier, string displayName)
        {
            Symbol = symbol;
            Underlying = underlying;
            Currency = currency;
            Exchange = exchange;
            InstrumentType = instrumentType;
            PutOrCall = putOrCall;
            Strike = strike;
            ExpiryDate = expiryDate;
            Multiplier = multiplier;
            DisplayName = displayName;
        }
    }
}
