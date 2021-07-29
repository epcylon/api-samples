using BridgeRock.CSharpExample.API.Values;
using BridgeRock.CSharpExample.ProtoStomp;
using QuantGate.API.Proto.Stealth;
using System;
using System.Diagnostics;

namespace BridgeRock.CSharpExample.API.Subscriptions
{
    internal class SentimentSubscription : GaugeSubscriptionBase<SentimentUpdate, Sentiment>
    {
        /// <summary>
        /// Module-level Identifier.
        /// </summary>
        private const string _moduleID = "SSub";

        public SentimentSubscription(ProtoStompClient client, string streamID, string symbol,
                                     string compression, bool receipt = false, uint throttleRate = 0) :
               base(client, SentimentUpdate.Parser, SubscriptionPath.GaugeSentiment,
                    streamID, symbol, compression, receipt, throttleRate)
        {
        }

        protected override object Preprocess(SentimentUpdate update)
        {
            double[] lengths;
            double[] colors;

            lengths = InterpolateTo55((int) update.Lengths.I, (int) update.Lengths.J, update.Lengths.X / 1000.0,
                                           update.Lengths.Y / 1000.0, update.Lengths.Z / 1000.0);
            colors = InterpolateTo55((int) update.Colors.I, (int) update.Colors.J, update.Colors.X / 1000.0,
                                      update.Colors.Y / 1000.0, update.Colors.Z / 1000.0);
            
            return Tuple.Create(lengths, colors);
        }

        protected override void HandleUpdate(SentimentUpdate update, object processed)
        {
            if (!(processed is Tuple<double[], double[]> converted))
                return;

            Values.TimeStamp = ProtoTimeEncoder.TimestampSecondsToDate(update.Timestamp);
            Values.Lengths = converted.Item1;
            Values.Colors = converted.Item2;
            Values.AvgLength = update.Lengths.Average / 1000.0;
            Values.AvgColor = update.Lengths.Average / 1000.0;
            Values.IsDirty = update.IsDirty;
        }

        #region Height/Color Interpolation        

        /// <summary>
        /// Interpolates a sentiment from it's compressed definition to 55 height or color values.
        /// </summary>
        /// <param name="i">The first point to calculate to.</param>
        /// <param name="j">The second point to calculate to.</param>
        /// <param name="x">Value at center.</param>
        /// <param name="y">Value at point i.</param>
        /// <param name="z">Value at point j.</param>
        /// <returns>A list of 55 heights or colors.</returns>
        public static double[] InterpolateTo55(int i, int j, double x, double y, double z)
        {
            double[] result = new double[Sentiment.TotalBars];
            double[] values;
            int next = 0;
            int remaining;

            try
            {
                // Calculate length between j and end.
                remaining = Sentiment.TotalBars - 1 - j;

                // Interpolate from 0 to i.
                values = new double[] { 2 * x - y, x, y, LinearInterpolate(i, y, j, z) };

                for (double index = 0; index <= i; index++, next++)
                    result[next] = CubicInterpolate(values, index / i);

                // Adjust j to simplify further calculations.
                j -= i;

                // Interpolate from i to j.
                values = new double[] { LinearInterpolate(j, y, j + i, x), y, z,
                                    LinearInterpolate(j, z, j + remaining, 0) };

                for (double index = 1; index <= j; index++, next++)
                    result[next] = CubicInterpolate(values, index / j);

                // Interpolate from j to end.
                values = new double[] { LinearInterpolate(remaining, z, remaining + j, y), z, 0, -z };

                for (double index = 1; index <= remaining; index++, next++)
                    result[next] = CubicInterpolate(values, index / remaining);
            }
            catch (Exception ex)
            {
                Trace.TraceError(_moduleID + ":IT55 - " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Returns the y-interpolation between x1,y1 and x2,y2 to distant point x5.
        /// </summary>
        /// <param name="x2">x-position of point 2.</param>
        /// <param name="y2">y-position of point 2.</param>
        /// <param name="x1">x-position of point 1.</param>
        /// <param name="y1">y-position of point 1.</param>
        /// <returns>The y-interpolation (y5).</returns>
        private static double LinearInterpolate(int x2, double y2, int x1, double y1)
        {
            if (x2 - x1 == 0)
                return (y2 + y1) / 2;
            else
                return x2 * (y2 - y1) / (x2 - x1) + y2;
        }

        /// <summary>
        /// Calculates the cubic interpolation between equidistant points p at x percent from p2 to p3.
        /// </summary>
        /// <param name="p">The equidistant points to calculate the interpolation between.</param>
        /// <param name="x">The distance between point p2 and point p3 as a percentage.</param>
        /// <returns>The cubic interpolation between equidistant points p at x percent from p2 to p3.</returns>
        private static double CubicInterpolate(double[] p, double x)
        {
            return p[1] + 0.5 * x * (p[2] - p[0] +
                (x * (2.0 * p[0] - 5.0 * p[1] + 4.0 * p[2] - p[3] + x * (3.0 * (p[1] - p[2]) + p[3] - p[0]))));
        }

        #endregion
    }
}
