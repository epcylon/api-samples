using QuantGate.API.Signals.Events;
using QuantGate.API.Signals.Proto.Stealth;
using QuantGate.API.Signals.ProtoStomp;
using QuantGate.API.Signals.Utilities;
using QuantGate.API.Signals.Values;
using System.Collections.Generic;

namespace QuantGate.API.Signals.Subscriptions
{
    internal class SearchSubscription : SubscriptionBase<SymbolSearchUpdate, SearchResults>
    {
        public SearchSubscription(APIClient client, string streamID, bool receipt = false, uint throttleRate = 0) :
            base(client, SymbolSearchUpdate.Parser,
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
                results.Add(new SearchResult
                {
                    Symbol = result.Symbol,
                    Underlying = result.Underlying,
                    Currency = result.Currency,
                    DisplayName = result.DisplayName,
                    Exchange = result.Exchange,
                    InstrumentType = (InstrumentType)result.InstrumentType,
                }); ;

            return results;
        }

        protected override SearchResults HandleUpdate(SymbolSearchUpdate update, object processed)
        {
            return new SearchResults
            {                 
                SearchTerm = update.SearchTerm,
                Results = processed as List<SearchResult>,
            };
        }
    }
}
