using QuantGate.API.Signals.Subscriptions;
using System;

namespace QuantGate.API.Signals
{
    internal class Subscription<V> : IDisposable
        where V : EventArgs
    {
        /// <summary>
        /// True if the object is disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Notifies that the object was updated (after complete update).
        /// </summary>
        public event EventHandler<V> Updated;

        /// <summary>
        /// Notifies that the object was updated (through the parent).
        /// </summary>
        internal EventHandler<V> ParentUpdatedEvent;

        /// <summary>
        /// Holds a reference to the subscription that these values are streamed from.
        /// </summary>
        internal ISubscription<V> Source { get; set; }

        /// <summary>
        /// Called whenever the values are finished updating.
        /// </summary>
        internal void SendUpdated(V values)
        {
            if (!_isDisposed)
            {
                Updated?.Invoke(Source.Client, values);
                ParentUpdatedEvent?.Invoke(Source.Client, values);
            }
        }

        /// <summary>
        /// The throttle rate of the subscription for these values (in ms).
        /// </summary>
        /// <remarks>Setting this value will change the throttle rate.</remarks>
        public int ThrottleRate
        {
            get => (int)Source.ThrottleRate;
            set
            {
                // If disposed, throw exception.
                if (_isDisposed)
                    throw new ObjectDisposedException("Subscription");

                // Set the throttle rate.
                Source.ThrottleRate = (uint)value;
            }
        }

        /// <summary>
        /// Handles the disposal of the subscription.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // Unsubscribe the subscription and set to disposed.
                Source.Unsubscribe();
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
