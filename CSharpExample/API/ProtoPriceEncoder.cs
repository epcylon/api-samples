namespace BridgeRock.CSharpExample.API
{
    /// <summary>
    /// Encodes prices into (unencoded) unsigned ProtoBuff varint values. 
    /// </summary>
    /// <remarks>
    /// Every 7 bits will be encoded to a full byte (with the eighth bit being used
    /// as a continuation bit), therefore we want to get the most out of every 7 
    /// bytes. To do this, we assume the following:
    /// 
    /// 1. All prices are positive values.
    /// 2. Prices are more likely to have few decimal places close to the point.
    /// 3. Prices will not require more than 16 significant digits (currencies, etc.).
    /// 
    /// Since most prices are likely to contain at least one decimal, the smallest
    /// number of decimal places included is one. For a single byte (7 bit varint)
    /// value, we assume one decimal place always, and the values are from 0.0 to 12.7.
    /// 
    /// Starting from a two byte value, the first bit determines the number of decimal
    /// places, and the remaining 13 bits form the (base 10) mantessa. So for a two-byte
    /// value, the range of values is from 0.0 to 819.1 and from 0.00 to 81.91. For
    /// lengths greater than this, the lowest number of decimal places is two (thus,
    /// none of the scale bits will be zero, allowing the x-byte values to be 
    /// distinguished from one another in edge-cases.
    /// 
    /// Two bits are used for the scale in a three-byte value, and the remaining 19 
    /// bits form the mantessa. So for a three-byte value, the range of values is from
    /// 0.00 to ‭5,242.87 up to 0.0000 to ‭52.4287.
    /// 
    /// Four byte values will use three bits for the scale and the remaining
    /// values will use four bits for the scale value. Generally, each level will gain
    /// two significant digits and double the scale to 16 decimal places (2^4).
    /// 
    /// A number that can be represented with a lower number of bytes will always be
    /// able to be represented in a different way with a higher number of bits. This
    /// overlap is not addressed to keep the algorithm simple. The value will always
    /// be represented by the least number of bytes possible, but higher scales are 
    /// not adjusted to further minimize the number of bits required.
    /// 
    /// NaN is represented as the smallest distinguishble overlapped zero value 
    /// possible (i.e it is a two byte zero value with two decimal places).
    /// 
    /// The up to 8-byte values adhere to the following pattern:
    /// 
    ///  Enc  ---------- Bits -------- ---------------- Range --------------   Sig
    /// Bytes Effective Scale Mantessa        Min                  Max       Digits
    /// 
    ///     1         7     0        7 00.0                            12.7    2-3   
    ///     2        14     1       13 01.28                          819.1    3-4
    ///     3        21     2       19  0.8192                       5242.87   5-6
    ///     4        28     3       25  0.00524288                 335544.31   7-8
    ///     5        35     4       31  0.0000000033554432       21474836.47   9-10
    ///     6        42     4       38  0.0000002147483648     2748779069.43  11-12
    ///     7        49     4       45  0.0000274877906944   351843720888.31  13-14
    ///     8        56     4       52  0.0035184372088832 45035996273704.95  15-16
    /// 
    /// (Encode bytes include varint continuation bits.)
    /// 
    /// Special values are as follows:
    /// 
    /// 0 = 0 [=0.0]
    /// NaN = 8192 [=0.00]
    /// </remarks>
    public static class ProtoPriceEncoder
    {
        #region Constants

        /// <summary>
        /// Maximum possible value.
        /// </summary>
        private static readonly ulong _maxValue;
        /// <summary>
        /// NaN is first zero value that has a non-zero scale.
        /// </summary>
        private static readonly ulong _nanValue;

        /// <summary>
        /// Maximum values for each level.
        /// </summary>
        private static readonly ulong[] _maxMantessas = new ulong[8];
        /// <summary>
        /// The lengths of each mantessa (in bits).
        /// </summary>
        private static readonly int[] _mantessaLengths = new int[8];
        /// <summary>
        /// The maximum allowable scale value for each level.
        /// </summary>
        private static readonly ulong[] _maxScales = new ulong[8];
        /// <summary>
        /// The mask for the defining bits of each level.
        /// </summary>
        private static readonly ulong[] _levelMask = new ulong[8];

        #endregion

        /// <summary>
        /// Calculate the constants for each level.
        /// </summary>
        static ProtoPriceEncoder()
        {
            ulong value = 0;
            int scaleBits = 0;

            for (int level = 0; level < 8; level++)
            {
                // Lengthen the value by 7.
                for (int bit = 0; bit < 7; bit++)
                    value = (value << 1) + 1;

                // Max scale = 2 ^ scale bits
                _maxScales[level] = 1UL << scaleBits;
                _mantessaLengths[level] = 7 * (level + 1) - scaleBits;
                _maxMantessas[level] = value >> scaleBits;
                _levelMask[level] = 0x7FUL << (7 * level);
                
                // Don't need more than 16.
                if (scaleBits < 4)
                    scaleBits++;
            }

            _maxValue = value;
            // NaN is first zero value that has a non-zero scale.
            _nanValue = 1 << (7 * 2 - 1);
        }

        /// <summary>
        /// Decode a proto price (long) back to a price (double).
        /// </summary>
        /// <param name="encoded">The encoded price.</param>
        /// <returns>The decoded price.</returns>
        public static double DecodePrice(ulong encoded)
        {
            ulong mantessa;
            ulong scale;

            // If NaN or too large, return as NaN.
            if (encoded == _nanValue || encoded > _maxValue)
                return double.NaN;

            for (int level = 7; level > -1; level--)
                if ((encoded & _levelMask[level]) != 0)
                {
                    // Get the mantessa and scale.
                    mantessa = encoded & _maxMantessas[level];
                    scale = (encoded >> _mantessaLengths[level]) + 1;

                    // Below is faster than simple multiplication.
                    // Naive implementation: return mantessa * Math.pow(0.1, scale);
                    return decimal.ToDouble(
                        new decimal((int)(mantessa & 0xFFFFFFFF),
                                    (int)(mantessa >> 32),
                                    0, false, (byte)scale));
                }

            // If didn't hit any mask, the value is zero.
            return 0;
        }
    }
}
