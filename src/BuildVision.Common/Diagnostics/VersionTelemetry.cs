using System;
using System.Linq;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace BuildVision.Common.Diagnostics
{
    class VersionTelemetry : ITelemetryInitializer
    {
        private readonly string _appVersion;

        public VersionTelemetry()
        {
            _appVersion = typeof(DiagnosticsClient).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                                                           .FirstOrDefault(ama => string.Equals(ama.Key, "CloudBuildNumber", StringComparison.OrdinalIgnoreCase))
                                                           ?.Value;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = _appVersion;
        }
    }
}
