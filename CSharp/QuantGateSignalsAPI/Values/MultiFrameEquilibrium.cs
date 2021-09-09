namespace QuantGate.API.Signals.Values
{
    /// <summary>
    /// Holds Multiframe Equilibrium values. Will be updated by the stream with change notifications.
    /// Supply this object to the Unsubscribe method of the APIClient to stop the subscription.
    /// </summary>
    public class MultiframeEquilibrium : GaugeValueBase<MultiframeEquilibrium>
    {
        /// <summary>
        /// 5 minute value.
        /// </summary>
        public double Min5 { get; internal set; }

        /// <summary>
        /// 10 minute value.
        /// </summary>
        public double Min10 { get; internal set; }

        /// <summary>
        /// 15 minute value.
        /// </summary>
        public double Min15 { get; internal set; }

        /// <summary>
        /// 30 minute value.
        /// </summary>
        public double Min30 { get; internal set; }

        /// <summary>
        /// 45 minute value.
        /// </summary>
        public double Min45 { get; internal set; }

        /// <summary>
        /// 60 minute value.
        /// </summary>
        public double Min60 { get; internal set; }

        /// <summary>
        /// 120 minute value.
        /// </summary>
        public double Min120 { get; internal set; }

        /// <summary>
        /// 180 minute value.
        /// </summary>
        public double Min180 { get; internal set; }

        /// <summary>
        /// 240 minute value.
        /// </summary>
        public double Min240 { get; internal set; }

        /// <summary>
        /// 1 day value.
        /// </summary>
        public double Day1 { get; internal set; }
    }
}
