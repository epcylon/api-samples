using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Values
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
        public string Symbol { get; internal set; }

        /// <summary>
        /// Underlying symbol.
        /// </summary>
        public string Underlying { get; internal set; }

        /// <summary>
        /// Currency the instrument is traded in.
        /// </summary>
        public string Currency { get; internal set; }

        /// <summary>
        /// Primary exchange (ISO 10383 MIC) the instrument is traded on.
        /// </summary>
        public string Exchange { get; internal set; }

        /// <summary>
        /// Display name of the instrument.
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Type of instrument. 
        /// </summary>
        public InstrumentType InstrumentType { get; internal set; }

        /// <summary>
        /// Right of an option, if an option (will be empty otherwise).
        /// </summary>
        public PutOrCall PutOrCall { get; internal set; }

        /// <summary>
        /// Strike price of an option, if an option (will be zero otherwise).
        /// </summary>
        public double Strike { get; internal set; }

        /// <summary>
        /// Expiry date of the instrument, if applicable
        /// </summary>
        public DateTime ExpiryDate { get; internal set; }

        /// <summary>
        /// Display name of the instrument.
        /// </summary>
        public double Multiplier { get; internal set; }

        /// <summary>
        /// Time zone of the primary exchange the instrument is traded on.
        /// </summary>
        public TimeZoneInfo TimeZone { get; internal set; }

        /// <summary>
        /// Tick ranges used to determine price levels.
        /// </summary>
        public List<TickRange> TickRanges { get; internal set; }

        /// <summary>
        /// Trading session end times and lengths for each day Sunday-Saturday, specified in the exchange time_zone.
        /// </summary>
        public List<TradingSession> TradingSessions { get; internal set; }

        /// <summary>
        /// Map of broker symbols according to broker (ib, cqg, dtniq, etc.).
        /// </summary>
        public Dictionary<string, string> BrokerSymbols { get; internal set; }
    }
}
