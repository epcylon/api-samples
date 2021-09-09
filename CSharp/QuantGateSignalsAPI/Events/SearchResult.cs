﻿namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// The details of an individual symbol in response to a symbol search request.
    /// </summary>
    public class SearchResult
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
        /// Type of instrument.
        /// </summary>
        public InstrumentType InstrumentType { get; internal set; }
        /// <summary>
        /// Primary exchange (ISO 10383 MIC) the instrument is traded on.
        /// </summary>
        public string Exchange { get; internal set; }
        /// <summary>        
        /// Display name of the instrument.
        /// </summary>
        public string DisplayName { get; internal set; }
    }
}