using System;
using System.Diagnostics;
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
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            _appVersion = versionInfo.ProductVersion.ToString();
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = _appVersion;
        }
    }
}
