namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Tick range used to determine price levels.
    /// </summary>
    public class TickRange
    {
        /// <summary>
        /// Start of the tick range.
        /// </summary>
        public double Start { get; internal set; }
        /// <summary>
        /// Tick value at this range.
        /// </summary>
        public double Tick { get; internal set; }
        /// <summary>
        /// Denominator for fractional formats.
        /// </summary>
        public int Denominator { get; internal set; }
        /// <summary>
        /// Number of decimals in decimal format.
        /// </summary>
        public int Decimals { get; internal set; }
        /// <summary>
        /// Format to use (Decimal, Fraction, or Tick).
        /// </summary>
        public TickFormat Format { get; internal set; }
    }
}
