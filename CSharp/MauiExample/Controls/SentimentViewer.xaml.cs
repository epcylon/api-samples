using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;

namespace BridgeRock.MauiExample.Controls
{
    public partial class SentimentViewer : Grid
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SVwr";

        #region Spectrum Constants

        /// <summary>
        /// The basic horizontal line brush to use.
        /// </summary>
        public static readonly SolidColorBrush _hLineBrush;
        /// <summary>
        /// The base line brush to use for the fill.
        /// </summary>
        public static readonly SolidColorBrush _baselineFill;

        /// <summary>
        /// The outline stroke for any of the peaks.
        /// </summary>
        public static readonly SolidColorBrush _peakStroke;
        /// <summary>
        /// The fill for top peak.
        /// </summary>
        public static readonly SolidColorBrush _topPeakFill;
        /// <summary>
        /// The fill for bottom peak.
        /// </summary>
        public static readonly SolidColorBrush _bottomPeakFill;

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
        static SentimentViewer()
        {
            _hLineBrush = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            _baselineFill = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x00));
            _peakStroke = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A));
            _topPeakFill = new SolidColorBrush(Colors.Red);
            _bottomPeakFill = new SolidColorBrush(Colors.Blue);
            _inactiveColor = new SolidColorBrush(Color.FromRgb(0x27, 0x27, 0x26));

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
            return Color.FromRgb(CalculateRBElement(forValue, 0.7125),
                                 CalculateRBElement(forValue, 0.5), CalculateRBElement(forValue, 0.2875));
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

        public SentimentViewer()
        {
            InitializeComponent();
        }


    }
}