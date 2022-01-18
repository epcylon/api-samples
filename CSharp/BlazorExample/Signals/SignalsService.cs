using QuantGate.API.Signals;
using QuantGate.API.Signals.Events;

namespace BlazorExample.Signals
{
    public class SignalsService
    {
        private readonly APIClient _client;

        private readonly object _topSymbolsLock = new();
        private TopSymbol[] _topSymbols = Array.Empty<TopSymbol>();

        private readonly object _sentimentLock = new();
        private SentimentEventArgs? _sentiment = null;

        private TopSymbol[] TopSymbols
        {
            get 
            { 
                lock (_topSymbolsLock)
                    return _topSymbols;
            }
            set 
            {
                lock (_topSymbolsLock)
                    _topSymbols = value;
            }
        }

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

        public SignalsService()
        { 
            _client = new APIClient();
            _client.TopSymbolsUpdated += HandleTopSymbolsUpdated;
            _client.SentimentUpdated += HandleSentimentUpdated;            
            
            _client.Connect("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                            "eyJzdWIiOiJUZXN0QXBwIiwiaWF0IjoxNjMzMDEyMTUzLCJleHAiOjE2MzgyMz" +
                            "A0MDAsImF1ZCI6IjJXVWplb2JSWFJXOXBzTkRFY3hlMU1EOXd0ZGZkaDFDIn0." +
                            "xtykKWHxKwhopUkkyUm6eCa9qfQsGkhHEdAea9hdSz8");

            _client.SubscribeTopSymbols("ib");
            _client.SubscribeSentiment("AAPL");
        }        

        private void HandleTopSymbolsUpdated(object? sender, TopSymbolsEventArgs e)
        {
            TopSymbols = e.Symbols.ToArray();
        }

        private void HandleSentimentUpdated(object? sender, SentimentEventArgs e)
        {
            Sentiment = e;
        }

        public Task<TopSymbol[]> GetTop10Async()
        {
            return Task.FromResult((TopSymbol[])TopSymbols.Clone());
        }

        public Task<SentimentEventArgs?> GetSentiment()
        {
            return Task.FromResult(Sentiment);
        }
    }
}
