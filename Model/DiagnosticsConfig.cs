using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace AlgorandKMDServer.Model
{
    /// <summary>
    /// Diagnostics configuration
    /// </summary>
    public class DiagnosticsConfig
    {
        /// <summary>
        /// Service name
        /// </summary>
        public const string ServiceName = "participation-server";
        /// <summary>
        /// Activity source
        /// </summary>
        public static ActivitySource ActivitySource = new ActivitySource(ServiceName);
        /// <summary>
        /// Meter
        /// </summary>
        public static Meter Meter = new(ServiceName);
        /// <summary>
        /// Counter
        /// </summary>
        public static Counter<long> RequestCounter = Meter.CreateCounter<long>("app.request_counter");
    }
}
