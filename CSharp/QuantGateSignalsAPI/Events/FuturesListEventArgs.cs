namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Futures List response details.
    /// </summary>
    public class FuturesListEventArgs : SubscriptionEventArgs
    {
        /// <summary>
        /// The underlying for which futures were requested.
        /// </summary>
        public string Underlying { get; }
        /// <summary>
        /// The currency of the futures that were requested.
        /// </summary>
        public string Currency { get; }
        public DataStream Stream { get; }
        /// <summary>
        /// The list of futures returned.
        /// </summary>
        public IReadOnlyList<InstrumentBase> Futures { get; }

        /// <summary>
        /// Creates a new FuturesListEventArgs instance.
        /// </summary>
        /// <param name="underlying">The underlying for which futures were requested.</param>
        /// <param name="currency">The currency of the futures that were requested.</param>
        /// <param name="futures">The list of futures returned.</param>
        internal FuturesListEventArgs(string underlying, string currency, DataStream stream,
                                      List<InstrumentBase> futures) : base(null)
        {
            Underlying = underlying;
            Currency = currency;
            Stream = stream;
            Futures = futures;
        }

        /// <summary>
        /// Creates a new FuturesListEventArgs instance.
        /// </summary>
        /// <param name="underlying">The underlying for which futures were requested.</param>
        /// <param name="currency">The currency of the futures that were requested.</param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>
        internal FuturesListEventArgs(string underlying, string currency, DataStream stream,
                                      SubscriptionError error) : base(error)
        {
            Underlying = underlying;
            Currency = currency;
            Stream = stream;
            Futures = new List<InstrumentBase>();
        }
    }
}
