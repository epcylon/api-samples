using System;

namespace QuantGate.API.Signals.Events
{
    /// <summary>
    /// Holds Multiframe Equilibrium values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class MultiframeEquilibriumEventArgs : GaugeEventArgs
    {
        /// <summary>
        /// 5 minute value.
        /// </summary>
        public double Min5 { get; }

        /// <summary>
        /// 10 minute value.
        /// </summary>
        public double Min10 { get; }

        /// <summary>
        /// 15 minute value.
        /// </summary>
        public double Min15 { get; }

        /// <summary>
        /// 30 minute value.
        /// </summary>
        public double Min30 { get; }

        /// <summary>
        /// 45 minute value.
        /// </summary>
        public double Min45 { get; }

        /// <summary>
        /// 60 minute value.
        /// </summary>
        public double Min60 { get; }

        /// <summary>
        /// 120 minute value.
        /// </summary>
        public double Min120 { get; }

        /// <summary>
        /// 180 minute value.
        /// </summary>
        public double Min180 { get; }

        /// <summary>
        /// 240 minute value.
        /// </summary>
        public double Min240 { get; }

        /// <summary>
        /// 1 day value.
        /// </summary>
        public double Day1 { get; }

        /// <summary>
        /// Creates a new MultiframeEquilibriumEventArgs instance.
        /// </summary>
        /// <param name="symbol">The symbol being subscribed to for this gauge.</param>
        /// <param name="timestamp">Timestamp of the latest update.</param>
        /// <param name="min5">5 minute value.</param>
        /// <param name="min10">10 minute value.</param>
        /// <param name="min15">15 minute value.</param>
        /// <param name="min30">30 minute value.</param>
        /// <param name="min45">45 minute value.</param>
        /// <param name="min60">60 minute value.</param>
        /// <param name="min120">120 minute value.</param>
        /// <param name="min180">180 minute value.</param>
        /// <param name="min240">240 minute value.</param>
        /// <param name="day1">1 day value.</param>
        /// <param name="isDirty">
        /// Whether the data used to generate this gauge value is potentially dirty 
        /// (values are missing) or stale (not the most recent data).
        /// </param>
        /// <param name="error">Holds error information, if a subscription error occured.</param>        
        internal MultiframeEquilibriumEventArgs(string symbol, DataStream stream, DateTime timestamp, double min5, double min10,
                                                double min15, double min30, double min45, double min60,
                                                double min120, double min180, double min240, double day1,
                                                bool isDirty, SubscriptionError error = null) :
            base(symbol, stream, timestamp, isDirty, error)
        {
            Min5 = min5;
            Min10 = min10;
            Min15 = min15;
            Min30 = min30;
            Min45 = min45;
            Min60 = min60;
            Min120 = min120;
            Min180 = min180;
            Min240 = min240;
            Day1 = day1;
        }
    }
}
