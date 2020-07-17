using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace BuildVision.Common.Diagnostics
{
    public static class DiagnosticsClient
    {
        private static bool _initialized;
        private static TelemetryClient _client;

        public static bool ParticipateInTelemetry { get; set; } = true;

        public static void Initialize(string edition, string vsVersion, string apiKey)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                TelemetryConfiguration.Active.InstrumentationKey = apiKey;
                TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = Debugger.IsAttached;
                TelemetryConfiguration.Active.TelemetryInitializers.Add(new VersionTelemetry());
                TelemetryConfiguration.Active.TelemetryInitializers.Add(new SessionTelemetry(vsVersion, edition));

                _initialized = true;
                _client = new TelemetryClient();
            }
        }

        public static void Flush()
        {
            try
            {
                if (!_initialized || !ParticipateInTelemetry)
                {
                    return;
                }

                _client.Flush();
                // Allow time for flushing:
                System.Threading.Thread.Sleep(1000);
            }
            catch { }
        }

        public static void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            try
            {
                if (!_initialized || !ParticipateInTelemetry)
                {
                    return;
                }

                _client.TrackEvent(eventName, properties, metrics);
            }
            catch { }
        }

        public static void TrackTrace(string trace)
        {
            try
            {
                if (!_initialized || !ParticipateInTelemetry)
                {
                    return;
                }

                _client.TrackTrace(trace);
            }
            catch { }
        }

        public static void TrackException(Exception exception)
        {
            try
            {
                if (!_initialized || !ParticipateInTelemetry)
                {
                    return;
                }

                _client.TrackException(exception);
            }
            catch { }
        }

        public static void TrackPageView(string pageName)
        {
            try
            {
                if (!_initialized || !ParticipateInTelemetry)
                {
                    return;
                }

                _client.TrackPageView(pageName);
            }
            catch { }
        }
    }
}
