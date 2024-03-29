﻿using Epcylon.Net.APIs.Account;
using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;
using Environments = Epcylon.Net.APIs.Account.Environments;

namespace BlazorExample.Signals
{
    /// <summary>
    /// Connects to the Signals API and allows for results to be retrieved.
    /// </summary>
    public class SignalsService
    {
        /// <summary>
        /// The main API client connection (this is a single server connection for all pages served).
        /// </summary>
        private readonly APIClient _client;

        /// <summary>
        /// Lock object to access top symbols with.
        /// </summary>
        private readonly object _topSymbolsLock = new();

        private readonly Dictionary<string, TopSymbol[]> _topSymbols = new() 
        { 
            ["ib"] = Array.Empty<TopSymbol>(),
            ["tradestation"] = Array.Empty<TopSymbol>() 
        };

        /// <summary>
        /// Lock object to access sentiment values with.
        /// </summary>
        private readonly object _sentimentLock = new();
        /// <summary>
        /// Holds the current 50t AAPL sentiment recieved from the server.
        /// </summary>
        private SentimentEventArgs? _sentiment = null;

        /// <summary>
        /// Sets the current list of top symbols.
        /// </summary>
        /// <remarks>
        /// This will handle the lock on the list. Note that TopSymbol items are immutable.
        /// </remarks>
        private void SetTopSymbols(string broker, TopSymbol[] value)            
        {
            lock (_topSymbolsLock)
                _topSymbols[broker] = value;
        }

        /// <summary>
        /// Returns the current list of top symbols.
        /// </summary>
        /// <remarks>
        /// This will handle the lock on the list. Note that TopSymbol items are immutable.
        /// </remarks>
        private TopSymbol[] GetTopSymbols(string broker)
        {
            lock (_topSymbolsLock)
                return _topSymbols[broker];
        }

        /// <summary>
        /// Sets or returns the current 50t AAPL sentiment values.
        /// </summary>
        /// <remarks>
        /// This will handle the lock on the object. Note that SentimentEventArgs items are immutable.
        /// </remarks>
        private SentimentEventArgs? Sentiment
        {
            get
            {
                lock (_sentimentLock)
                    return _sentiment;
            }
            set
            {
                lock (_sentimentLock)
                    _sentiment = value;
            }
        }

        /// <summary>
        /// Creates a new SignalsService instance.
        /// </summary>
        public SignalsService()
        {
            // Create the API client, and wire up the handlers.
            _client = new APIClient(
                new ConnectionToken(Environments.Production, "{username here}", "{password here}"));

            _client.TopSymbolsUpdated += HandleTopSymbolsUpdated;
            _client.SentimentUpdated += HandleSentimentUpdated;

            // Connect to the API client.           
            _client.Connect();

            // Subscribe to top symbols and to the 50t AAPL sentiment feed.
            _client.SubscribeTopSymbols("ib", InstrumentType.Future);
            _client.SubscribeTopSymbols("tradestation", InstrumentType.CommonStock);
            _client.SubscribeSentiment("AAPL");
        }        

        /// <summary>
        /// Called whenever a TopSymbols update is received.
        /// </summary>
        /// <param name="sender">The object sending the update.</param>
        /// <param name="e">The TopSymbols update event arguments.</param>
        private void HandleTopSymbolsUpdated(object? sender, TopSymbolsEventArgs e)
        {
            SetTopSymbols(e.Broker, e.Symbols.ToArray());
        }

        /// <summary>
        /// Called whenever a 50t AAPL Sentiment update is received.
        /// </summary>
        /// <param name="sender">The object sending the update.</param>
        /// <param name="e">The 50t AAPL update event arguments.</param>
        private void HandleSentimentUpdated(object? sender, SentimentEventArgs e)
        {
            Sentiment = e;
        }

        /// <summary>
        /// Returns the current Top10 Symbols list asynchronously.
        /// </summary>
        public Task<TopSymbol[]> GetTop10Async(string broker) => Task.FromResult((TopSymbol[])GetTopSymbols(broker).Clone());        

        /// <summary>
        /// Returns the current 50t Sentiment value asynchronously.
        /// </summary>
        public Task<SentimentEventArgs?> GetSentiment() => Task.FromResult(Sentiment);
    }
}
