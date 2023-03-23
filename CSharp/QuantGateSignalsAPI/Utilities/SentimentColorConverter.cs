using System.Diagnostics;

namespace QuantGate.API.Signals.Utilities
{
    /// <summary>
    /// Used to convert between sentiment color values and RBG, ARGB, or RGBA color codes.
    /// </summary>
    public class SentimentColorConverter
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SCCvtr";

        /// <summary>
        /// Holds a list of precalculated RGB values.
        /// </summary>
        private static readonly string[] _rgb = new string[401];
        /// <summary>
        /// Holds a list of precalculated ARGB values.
        /// </summary>
        private static readonly string[] _argb = new string[401];
        /// <summary>
        /// Holds a list of precalculated RGBA values.
        /// </summary>
        private static readonly string[] _rgba = new string[401];

        /// <summary>
        /// Creates the static solors when the first item is created.
        /// </summary>
        static SentimentColorConverter()
        {
            string rgb;

            for (int index = 0; index <= 400; index++)
            {
                rgb = RedBlueColor(index / 400.0);
                _rgb[index] = "#" + rgb;
                _argb[index] = "#FF" + rgb;
                _rgba[index] = "#" + rgb + "FF";
            }
        }

        /// <summary>
        /// Gets the RGB version of the color associated with the sentiment color value given.
        /// </summary>
        /// <param name="color">The sentiment color value to convert.</param>
        /// <param name="exact">True if we need the exact color (default false).</param>
        /// <returns>The RGB version of the sentiment color.</returns>
        public static string GetRGBColor(double color, bool exact = false)
        {
            if (!exact)
                return _rgb[(int)((color + 1) * 200)];
            else
                return "#" + RedBlueColor((color + 1) * 0.5);
        }

        /// <summary>
        /// Gets the ARGB version of the color associated with the sentiment color value given.
        /// </summary>
        /// <param name="color">The sentiment color value to convert.</param>
        /// <param name="exact">True if we need the exact color (default false).</param>
        /// <returns>The ARGB version of the sentiment color.</returns>
        public static string GetARGBColor(double color, bool exact = false)
        {
            if (!exact)
                return _argb[(int)((color + 1) * 200)];
            else
                return "#FF" + RedBlueColor((color + 1) * 0.5);
        }

        /// <summary>
        /// Gets the RGBA version of the color associated with the sentiment color value given.
        /// </summary>
        /// <param name="color">The sentiment color value to convert.</param>
        /// <param name="exact">True if we need the exact color (default false).</param>
        /// <returns>The RGBA version of the sentiment color.</returns>
        public static string GetRGBAColor(double color, bool exact = false)
        {
            if (!exact)
                return _rgba[(int)((color + 1) * 200)];
            else
                return "#" + RedBlueColor((color + 1) * 0.5) + "FF";
        }

        /// <summary>
        /// This method is used to calculate an RGB value as an integer from a double.
        /// </summary>
        /// <param name="forValue">The value to calculate for.</param>
        /// <returns>Returns an color representing the RGB value for the provided value.</returns>        
        private static string RedBlueColor(double forValue)
        {
            //The final color is the combination of the RGB elements.
            return CalculateRBElement(forValue, 0.7125) +
                   CalculateRBElement(forValue, 0.5) +
                   CalculateRBElement(forValue, 0.2875);
        }

        /// <summary>
        /// SlidePct is the mid-point where the max value of that color is place, in percentage points.
        /// </summary>
        /// <param name="forValue">The value to use for the calculation.</param>
        /// <param name="slidePercent">The percentage to slide.</param>
        /// <returns>Returns a value from 00 to FF.</returns>        
        private static string CalculateRBElement(double forValue, double slidePercent)
        {
            double adjustedValue;
            byte asByte;

            try
            {
                // Calculate the main adjusted value.
                adjustedValue = Math.Abs(forValue - slidePercent) * 1166 - 146;

                if (adjustedValue > 255.0)
                {
                    // If we're too high, use the max.
                    return "FF";
                }
                else if (adjustedValue < 0.0)
                {
                    // If we're too low, use the min.
                    return "00";
                }
                else
                {
                    // Otherwise, convert to a byte.
                    asByte = Convert.ToByte(adjustedValue);

                    // Return as two character byte string.
                    if (asByte < 16)
                        return "0" + Convert.ToString(asByte, 16);
                    else
                        return Convert.ToString(asByte, 16);
                }

            }
            catch (Exception ex)
            {
                //Use the adjusted value.
                Trace.TraceError(_moduleID + ":CRBE - " + ex.Message);
                return "00";
            }
        }
    }
}
