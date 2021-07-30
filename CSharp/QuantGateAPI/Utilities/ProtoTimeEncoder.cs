using System;

namespace QuantGate.API.Utilities
{
    internal static class ProtoTimeEncoder
    {
        /// <summary>
        /// January 1, 1800 date.
        /// </summary>
        private static readonly DateTime _date1800 = new DateTime(1800, 1, 1);
        /// <summary>
        /// The number of ticks since January 1, 1800
        /// </summary>
        private static readonly long _ticks1800 = _date1800.Ticks;
        
        public static ulong DateToTimestampSeconds(DateTime date)
        {
            return (ulong)(date.Ticks - _ticks1800) / TimeSpan.TicksPerSecond;
        }

        public static DateTime TimestampSecondsToDate(ulong timestamp)
        {
            return new DateTime(_ticks1800 + (long)(timestamp * TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        }

        public static DateTime DaysToDate(ulong days)
        {
            return _date1800.AddDays(days);
        }
    }
}
