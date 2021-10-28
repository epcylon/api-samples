using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Instrument details. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient after values are received 
    /// to end the subscription.
    /// </summary>
    public class InstrumentEventArgs : EventArgs
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
        /// Time zone of the primary exchange the instrument is traded on.
        /// </summary>
        public TimeZoneInfo TimeZone { get; }

        /// <summary>
        /// Tick ranges used to determine price levels.
        /// </summary>
        public IReadOnlyList<TickRange> TickRanges { get; }

        /// <summary>
        /// Trading session end times and lengths for each day Sunday-Saturday, specified in the exchange time_zone.
        /// </summary>
        public IReadOnlyList<TradingSession> TradingSessions { get; }

        /// <summary>
        /// Map of broker symbols according to broker (ib, cqg, dtniq, etc.).
        /// </summary>
        public IReadOnlyDictionary<string, string> BrokerSymbols { get; }

        /// <summary>
        /// Describes an error if the instrument details request failed.
        /// </summary>
        public string ErrorMessage { get; }

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
        /// <param name="timeZone">Time zone of the primary exchange the instrument is traded on.</param>
        /// <param name="tickRanges">Tick ranges used to determine price levels.</param>
        /// <param name="tradingSessions">
        /// Trading session end times and lengths for each day Sunday-Saturday, specified in the exchange time_zone.
        /// </param>
        /// <param name="brokerSymbols">Map of broker symbols according to broker (ib, cqg, dtniq, etc.).</param>
        internal InstrumentEventArgs(string symbol, string underlying, string currency, string exchange,
                                     InstrumentType instrumentType, PutOrCall putOrCall, double strike, 
                                     DateTime expiryDate, double multiplier, string displayName,
                                     TimeZoneInfo timeZone, List<TickRange> tickRanges,
                                     List<TradingSession> tradingSessions, Dictionary<string, string> brokerSymbols)
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
            TimeZone = timeZone;
            TickRanges = tickRanges;
            TradingSessions = tradingSessions;
            BrokerSymbols = brokerSymbols;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Creates a new instance when an error occurs;
        /// </summary>
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="errorMessage">Describes the error if the instrument details request failed.</param>
        public InstrumentEventArgs(string symbol, string errorMessage)
        {
            Symbol = symbol;
            Underlying = string.Empty;
            Currency = string.Empty;
            Exchange = string.Empty;
            InstrumentType = InstrumentType.NoInstrument;
            PutOrCall = PutOrCall.NoPutCall;
            Strike = 0;
            ExpiryDate = default;
            Multiplier = 0;
            DisplayName = string.Empty;
            TimeZone = TimeZoneInfo.Utc;
            TickRanges = new List<TickRange>();
            TradingSessions = new List<TradingSession>();
            BrokerSymbols = new Dictionary<string, string>();
            ErrorMessage = errorMessage;
        }
    }
}
