using System;

namespace QuantGate.API.Values
{
    /// <summary>
    /// Trading session end times and lengths for each day Sunday-Saturday, specified in the exchange Time Zone.
    /// </summary>
    /// <remarks>
    /// The close times are within the day of the week associated with the index within the array of 
    /// trading_sessions. Open times may occur in the day prior to the close times and are calculated
    /// by subtracting the length from the close time.
    /// </remarks>
    public class TradingSession
    {
        /// <summary>
        /// Number of minutes per one day.
        /// </summary>
        private const int _minutesPerDay = (int)(TimeSpan.TicksPerDay / TimeSpan.TicksPerMinute);

        /// <summary>
        /// The day of the week that the trading session ends on.
        /// </summary>
        public DayOfWeek DayOfWeek { get; }
        /// <summary>
        /// The close time of the day.
        /// </summary>
        public TimeSpan Close { get; }
        /// <summary>
        /// The length of the trading session.
        /// </summary>
        public TimeSpan Length { get; }
        /// <summary>
        /// The open minute of the day
        /// </summary>
        public TimeSpan Open { get; }
        /// <summary>
        /// Returns true if the start of the trading session is on the previous day of the week.
        /// </summary>
        public bool StartsOnPrevious => Length > Close;

        /// <summary>
        /// Creates a new trading session instance.
        /// </summary>
        /// <param name="dayOfWeek">The day of the week that the trading session ends on.</param>
        /// <param name="close">The close time of the day.</param>
        /// <param name="length">The length of the trading session.</param>
        public TradingSession(DayOfWeek dayOfWeek, int close, int length)
        {
            DayOfWeek = dayOfWeek;
            Close = TimeSpan.FromMinutes(close);
            Length = TimeSpan.FromMinutes(length);
            Open = TimeSpan.FromMinutes((_minutesPerDay + close - length) % _minutesPerDay);            
        }
    }
}
