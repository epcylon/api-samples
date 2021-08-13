using QuantGate.API.Signals.Subscriptions;
using QuantGate.API.Signals.Values;
using System;
using System.Linq;

namespace QuantGate.API.Signals
{
    /// <summary>
    /// Used to search for symbols and return symbol search results.
    /// </summary>
    public class SymbolSearch : IDisposable
    {
        /// <summary>
        /// Is this object currently disposed of?
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// Raised whenever a new symbol search result is received.
        /// </summary>
        public event EventHandler<SearchUpdateEventArgs> Updated = delegate { };

        /// <summary>
        /// The internal search subscription object tied to the search.
        /// </summary>
        internal SearchSubscription Subscription { get; }

        /// <summary>
        /// Creates a new SymbolSearch instance.
        /// </summary>
        /// <param name="subscription">The subscription to subscribe with and send requests to.</param>
        internal SymbolSearch(SearchSubscription subscription)
        {
            Subscription = subscription;
            Subscription.Values.Updated += HandleValuesUpdate;
        }

        /// <summary>
        /// Called whenever the values are updated.
        /// </summary>
        private void HandleValuesUpdate(object sender, EventArgs e)
        {
            // Pass the event through.
            Updated(this, new SearchUpdateEventArgs(Subscription.Values.SearchTerm, 
                                                   Subscription.Values.Results.ToList()));
        }

        /// <summary>
        /// Requests symbols that match a specific term and (optionally) a specific broker. 
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <param name="broker">Broker to get the results for. If supplied, must match a valid broker type string.</param>
        public void Search(string term, string broker) => Subscription.Search(term, broker);

        /// <summary>
        /// Handles the actual logic of the dispose method.
        /// </summary>
        /// <param name="disposing">True if called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                Subscription.Unsubscribe();                
                _isDisposed = true;
            }
        }               

        /// <summary>
        /// Used to dispose of this object and clean up subscriptions.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SymbolSearch()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
    }
}
