using QuantGate.API.Signals.Subscriptions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// The base class for all values (mainly just a basic INotifyPropertyChanged implementation).
    /// </summary>
    public abstract class ValueBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Holds a reference to the subscription that these values are streamed from.
        /// </summary>
        internal ISubscription Subscription { get; set; }

        /// <summary>
        /// Notifies when one of the properties changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies that the object was updated (after complete update).
        /// </summary>
        public event EventHandler Updated;
        
        /// <summary>
        /// This method is called by the Set accessor of each property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <remarks>
        /// The CallerMemberName attribute that is applied to the optional propertyName  
        /// parameter causes the property name of the caller to be substituted as an argument.  
        /// </remarks>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// Called whenever the values are finished updating.
        /// </summary>
        internal void SendUpdated() => Updated?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Unsubscribe from the subscription.
        /// </summary>
        internal void Unsubscribe() => Subscription.Unsubscribe();

        /// <summary>
        /// The throttle rate of the subscription for these values (in ms).
        /// </summary>
        /// <remarks>Setting this value will change the throttle rate.</remarks>
        public int ThrottleRate
        {
            get => (int)Subscription.ThrottleRate;
            set => Subscription.ThrottleRate = (uint)value;
        }
    }
}
