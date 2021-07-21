using System.Threading;

namespace BridgeRock.CSharpExample.ProtoStomp
{
    /// <summary>
    /// Used to generate IDs for the STOMP protocol.
    /// </summary>
    public static class IDGenerator
    {
        /// <summary>
        /// The next ID to use.
        /// </summary>
        private static long _counter = 0;

        /// <summary>
        /// Returns the next ID to use.
        /// </summary>
        /// <returns>The next ID to use.</returns>
        public static ulong NextID
        {
            get
            {
                // Return the new ID.
                return unchecked((ulong)Interlocked.Increment(ref _counter));
            }
        }
    }
}