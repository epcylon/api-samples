namespace QuantGate.API.Signals.Events;

/// <summary>
/// Base event args class for all subscriptions.
/// </summary>
public class SubscriptionEventArgs : EventArgs
{
    /// <summary>
    /// Reference object tied to this subscription.
    /// </summary>
    public object Reference { get; protected internal set; }

    /// <summary>
    /// Holds error information, if a subscription error occured.
    /// </summary>
    public SubscriptionError Error { get; }

    /// <summary>
    /// Creates a new GaugeArgsBase instance.
    /// </summary>        
    /// <param name="reference">Reference object tied to this subscription.</param>        
    /// <param name="error">Subscription error information, if an error occured.</param>
    internal SubscriptionEventArgs(SubscriptionError error)
    {
        Error = error;
    }

    /// <summary>
    /// Creates a copy of this object.
    /// </summary>
    /// <returns>A copy of this object.</returns>
    protected internal virtual object Clone() => MemberwiseClone();
}
