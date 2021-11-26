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
        public SearchSubscription(APIClient client, EventHandler<SearchResultsEventArgs> handler,
                                  string streamID, bool receipt = false, uint throttleRate = 0) :
            base(client, SymbolSearchUpdate.Parser, handler,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnSymbolSearch, streamID).Destination,
                 receipt, throttleRate)
        {
        }

        public void Search(string term, string broker)
        {
            Client.Send(new ProtoStompSend(Client, Destination + '/' + term + '/' + broker));
        }

        protected override object Preprocess(SymbolSearchUpdate update)
        {
            List<SearchResult> results = new List<SearchResult>();

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
            new SearchResultsEventArgs(update.SearchTerm, (List<SearchResult>)processed);        

        protected override SearchResultsEventArgs WrapError(SubscriptionError error) =>
            new SearchResultsEventArgs(string.Empty, new List<SearchResult>(), error);
    }
}
