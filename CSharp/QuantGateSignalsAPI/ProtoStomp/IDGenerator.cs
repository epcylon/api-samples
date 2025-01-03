namespace QuantGate.API.Signals.ProtoStomp;

/// <summary>
/// Used to generate IDs for the STOMP protocol.
/// </summary>
internal class IDGenerator
{
    /// <summary>
    /// The next ID to use.
    /// </summary>`
    private long _counter = 0;

    /// <summary>
    /// Resets the ID to zero (only use after reconnection).
    /// </summary>
    public void Reset() => Interlocked.Exchange(ref _counter, 0);

    /// <summary>
    /// Returns the next ID to use.
    /// </summary>
    /// <returns>The next ID to use.</returns>
    public ulong NextID => unchecked((ulong)Interlocked.Increment(ref _counter));
}