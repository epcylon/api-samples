using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds the details of an instrument.
    /// </summary>
    public class Instrument : InstrumentBase
    {
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
        internal Instrument(string symbol, string underlying, string currency, string exchange,
                            InstrumentType instrumentType, PutOrCall putOrCall, double strike,
                            DateTime expiryDate, double multiplier, string displayName,
                            TimeZoneInfo timeZone, List<TickRange> tickRanges,
                            List<TradingSession> tradingSessions, Dictionary<string, string> brokerSymbols) : 
            base(symbol, underlying, currency, exchange, instrumentType, 
                 putOrCall, strike, expiryDate, multiplier, displayName)
        {
            TimeZone = timeZone;
            TickRanges = tickRanges;
            TradingSessions = tradingSessions;
            BrokerSymbols = brokerSymbols;
        }
    }
}
