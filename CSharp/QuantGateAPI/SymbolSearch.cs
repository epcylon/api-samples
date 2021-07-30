using QuantGate.API.Subscriptions;
using QuantGate.API.Values;
using System;
using System.Linq;

namespace QuantGate.API
{
    public class SymbolSearch : IDisposable
    {
        private bool _isDisposed = false;

        public event EventHandler<SearchUpdateEventArgs> Update = delegate { };

        internal SearchSubscription Subscription { get; }

        internal SymbolSearch(SearchSubscription subscription)
        {
            Subscription = subscription;
            Subscription.Values.Updated += HandleValuesUpdate;
        }

        private void HandleValuesUpdate(object sender, EventArgs e)
        {
            Update(this, new SearchUpdateEventArgs(Subscription.Values.SearchTerm, 
                                                   Subscription.Values.Results.ToList()));
        }

        public void Search(string term, string broker) => Subscription.Search(term, broker);

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                Subscription.Unsubscribe();                
                _isDisposed = true;
            }
        }               

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~SymbolSearch()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
    }
}
