using System;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Instrument details.
    /// </summary>
    public class InstrumentEventArgs : SubscriptionEventArgs
    {
        /// <summary>
        /// Symbol as listed by the QuantGate servers.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// The details of the instrument.
        /// </summary>
        public Instrument Details { get; }

        /// <summary>
        /// Creates a new InstrumentEventArgs instance.
        /// </summary>
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="details">The details of the instrument.</param>
        internal InstrumentEventArgs(string symbol, Instrument details) : base(null)
        {
            Symbol = symbol;
            Details = details;
        }

        /// <summary>
        /// Creates a new instance when an error occurs;
        /// </summary>
        /// <param name="symbol">Symbol as listed by the QuantGate servers.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        public InstrumentEventArgs(string symbol, SubscriptionError error) : base(error)
        {
            Symbol = symbol;
            Details = null;
        }
    }
}
