using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.ProtoStomp;
using QuantGate.API.Signals.Utilities;
using System;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Subscriptions
{    
    internal class SearchSubscription : SubscriptionBase<SymbolSearchUpdate, SearchResultsEventArgs>
    {
        private readonly Queue<Tuple<string, string>> _queue = new();
        private bool _connected = false;

        public SearchSubscription(APIClient client, EventHandler<SearchResultsEventArgs> handler,
                                  string streamID, uint throttleRate = 0, object reference = null) :
            base(client, SymbolSearchUpdate.Parser, handler,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnSymbolSearch, streamID).Destination,
                 true, throttleRate, reference)
        {
            OnReceipt += HandleReceipt;
        }                

        public void Search(string term, string broker)
        {
            // Search or enqueue.
            if (_connected)
                Client.Send(new ProtoStompSend(Client, Destination + '/' + term + '/' + broker));
            else
                _queue.Enqueue(new Tuple<string, string>(term, broker));
        }

        private void HandleReceipt(IReceiptable obj)
        {
            // On receipt of the subscription, add all searches from the queue.
            _connected = true;
            foreach (Tuple<string, string> search in _queue)
                Search(search.Item1, search.Item2);
        }

        protected override object Preprocess(SymbolSearchUpdate update)
        {
            List<SearchResult> results = new();

            foreach (SymbolSearchResult result in update.Results)
                results.Add(new SearchResult(result.Symbol,
                                             result.Underlying,
                                             result.Currency,
                                             (InstrumentType)result.InstrumentType,
                                             result.Exchange,
                                             result.DisplayName));

            return results;
        }

        protected override SearchResultsEventArgs HandleUpdate(SymbolSearchUpdate update, object processed) =>
            new(update.SearchTerm, (List<SearchResult>)processed);        

        protected override SearchResultsEventArgs WrapError(SubscriptionError error) =>
            new(string.Empty, new List<SearchResult>(), error);
    }
}
