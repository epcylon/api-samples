using System.Diagnostics;
using MG = Microsoft.Maui.Graphics;

namespace QuantGate.MauiExample.Controls
{
    public partial class SentimentBar : Grid
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SBr";

        #region Spectrum Colors

        /// <summary>
        /// The inactive color for a spectrum line.
        /// </summary>
        private static readonly SolidColorBrush _inactiveColor;
        /// <summary>
        /// Holds a list of frozen brushes to use.
        /// </summary>
        private static readonly SolidColorBrush[] _brushes = new SolidColorBrush[401];

        #endregion

        #region Static Brush Setup

        /// <summary>
        /// Creates the static brushes when the first item is created.
        /// </summary>
        static SentimentBar()
        {
            _inactiveColor = new SolidColorBrush(MG.Color.FromRgb(0x27, 0x27, 0x26));

            for (int index = 0; index <= 400; index++)
                _brushes[index] = new SolidColorBrush(RedBlueColor(index / 400.0));
        }

        /// <summary>
        /// This method is used to calculate an RGB value as an integer from a double.
        /// </summary>
        /// <param name="forValue">The value to calculate for.</param>
        /// <returns>Returns an color representing the RGB value for the provided value.</returns>        
        private static Color RedBlueColor(double forValue)
        {
            //The final color is the combination of the RGB elements.
            return MG.Color.FromRgb(CalculateRBElement(forValue, 0.7125),
                                    CalculateRBElement(forValue, 0.5),
                                    CalculateRBElement(forValue, 0.2875));
        }

        /// <summary>
        /// SlidePct is the mid-point where the max value of that color is place, in percentage points.
        /// </summary>
        /// <param name="forValue">The value to use for the calculation.</param>
        /// <param name="slidePercent">The percentage to slide.</param>
        /// <returns>Returns a value from 0 to 255.</returns>        
        private static byte CalculateRBElement(double forValue, double slidePercent)
        {
            byte result;
            double adjustedValue;                                               // The main adjusted value.

            try
            {
                adjustedValue = Math.Abs(forValue - slidePercent) * 1166 - 146; // Calculate the value.

                if (adjustedValue > 255.0)                                      // If we're too high.
                    result = 255;                                               // Use the max.
                else if (adjustedValue < 0.0)                                   // If we're too low.
                    result = 0;                                                 // Use the min.
                else                                                            // Otherwise...
                    result = Convert.ToByte(adjustedValue);                     // Use the adjusted value.                
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":CRBE - " + ex.Message);
                //Use the adjusted value.
                result = Convert.ToByte(0);
            }

            return result;
        }

        #endregion

        public SentimentBar()
        {
            InitializeComponent();
        }

        private double _length = 0;
        public double Length
        {
            get => _length;
            set
            {
                if (_length != value)
                {
                    _length = value;

                    if (_length > 0)
                    {
                        topRow.Height = new GridLength(1000 * (1.0 - _length), GridUnitType.Star);
                        barRow.Height = new GridLength(1000 * _length, GridUnitType.Star);
                        bottomRow.Height = new GridLength(1000, GridUnitType.Star);
                    }
                    else
                    {
                        topRow.Height = new GridLength(1000, GridUnitType.Star);
                        barRow.Height = new GridLength(1000 * -_length, GridUnitType.Star);
                        bottomRow.Height = new GridLength(1000 * (1.0 + _length), GridUnitType.Star);
                    }
                }
            }
        }

        private double _color = 0;
        public double Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;

                    if (!double.IsNaN(_color))
                    {
                        // If this control is active, calculate color, calculate the new color.                            
                        rBar.Fill = _brushes[(int)((_color + 1) * 200)];
                    }
                    else
                    {
                        // Otherwise, use the inactive color.
                        rBar.Fill = _inactiveColor;
                    }
                }
            }
        }
    }
}