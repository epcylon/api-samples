using System.Threading;

namespace QuantGate.API.ProtoStomp
{
    /// <summary>
    /// Used to generate IDs for the STOMP protocol.
    /// </summary>
    public static class IDGenerator
    {
        /// <summary>
        /// The next ID to use.
        /// </summary>`
        private static long _counter = 0;

        /// <summary>
        /// Resets the ID to zero (only use after reconnection).
        /// </summary>
        public static void Reset() => Interlocked.Exchange(ref _counter, 0);        

        /// <summary>
        /// Returns the next ID to use.
        /// </summary>
        /// <returns>The next ID to use.</returns>
        public static ulong NextID => unchecked((ulong)Interlocked.Increment(ref _counter));
    }
}