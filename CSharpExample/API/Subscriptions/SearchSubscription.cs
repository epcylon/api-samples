using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using QuantGate.API.Proto.Stealth;
using System.Linq;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    public class SearchSubscription : SubscriptionBase<SymbolSearchUpdate, SearchResults>
    {
        public SearchSubscription(ProtoStompClient client, string streamID, bool receipt = false, uint throttleRate = 0) :
            base(client, SymbolSearchUpdate.Parser,
                 new ParsedDestination(SubscriptionType.Definition, SubscriptionPath.DefnSymbolSearch, streamID).Destination,
                 receipt, throttleRate)
        {
        }

        public void Search(string term, string broker)
        {
            Client.Send(new ProtoStompSend(Client, Destination + '/' + term + '/' + broker));
        }

        protected override void HandleUpdate(SymbolSearchUpdate update, object processed)
        {
            Values.SearchTerm = update.SearchTerm;
            Values.Results = update.Results.ToList();
        }
    }
}
