using System;

namespace QuantGate.API.Utilities
{
    public static class ProtoTimeEncoder
    {
        /// <summary>
        /// The number of ticks since January 1, 1800
        /// </summary>
        private static readonly long _ticks1800 = new DateTime(1800, 1, 1).Ticks;
        
        public static ulong DateToTimestampSeconds(DateTime date)
        {
            return (ulong)(date.Ticks - _ticks1800) / TimeSpan.TicksPerSecond;
        }

        public static DateTime TimestampSecondsToDate(ulong timestamp)
        {
            return new DateTime(_ticks1800 + (long)(timestamp * TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        }
    }
}
