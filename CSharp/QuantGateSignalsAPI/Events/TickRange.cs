namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Tick range used to determine price levels.
    /// </summary>
    public class TickRange
    {
        /// <summary>
        /// Start of the tick range.
        /// </summary>
        public double Start { get; }
        /// <summary>
        /// Tick value at this range.
        /// </summary>
        public double Tick { get; }
        /// <summary>
        /// Denominator for fractional formats.
        /// </summary>
        public int Denominator { get; }
        /// <summary>
        /// Number of decimals in decimal format.
        /// </summary>
        public int Decimals { get; }
        /// <summary>
        /// Format to use (Decimal, Fraction, or Tick).
        /// </summary>
        public TickFormat Format { get; }

        /// <summary>
        /// Creates a new TickRange instance.
        /// </summary>
        /// <param name="start">Start of the tick range.</param>
        /// <param name="tick">Tick value at this range.</param>
        /// <param name="denominator">Denominator for fractional formats.</param>
        /// <param name="decimals">Number of decimals in decimal format.</param>
        /// <param name="format">Format to use (Decimal, Fraction, or Tick).</param>
        internal TickRange(double start, double tick, int denominator, int decimals, TickFormat format)
        {
            Start = start;
            Tick = tick;
            Denominator = denominator;
            Decimals = decimals;
            Format = format;
        }
    }
}
