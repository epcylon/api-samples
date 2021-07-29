using QuantGate.API.Proto.Stealth;
using QuantGate.API.ProtoStomp;
using QuantGate.API.Utilities;
using QuantGate.API.Values;
using System.Collections.Generic;

namespace QuantGate.API.Subscriptions
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

            foreach (var result in update.Results)
                results.Add(new SearchResult
                {
                    Symbol = result.Symbol,
                    Underlying = result.Underlying,
                    Currency = result.Currency,
                    DisplayName = result.DisplayName,
                    Exchange = result.Exchange,
                    InstrumentType = result.InstrumentType,
                }); ;

            return results;
        }

        protected override void HandleUpdate(SymbolSearchUpdate update, object processed)
        {
            Values.SearchTerm = update.SearchTerm;
            Values.Results = processed as List<SearchResult>;
        }
    }
}
