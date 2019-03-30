using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace BuildVision.Common.Diagnostics
{
    class SessionTelemetry : ITelemetryInitializer
    {
        private readonly string _userName;
        private readonly string _operatingSystem = RuntimeInformation.OSDescription?.Replace("Microsoft ", ""); // Shorter description
        private readonly string _session = Guid.NewGuid().ToString();

        public SessionTelemetry()
        {
            try
            {
                using (var hash = SHA256.Create())
                {
                    var hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(Environment.MachineName + Environment.UserDomainName + Environment.UserName));
                    _userName = Convert.ToBase64String(hashBytes);
                }
            }
            catch
            {
                // No user id                
            }
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties["Environment"] = "release";
            // Always default to development if we're in the debugger
            if (Debugger.IsAttached)
            {
                telemetry.Context.GlobalProperties["Environment"] = "development";
            }

            if (_userName != null)
            {
                telemetry.Context.User.Id = _userName;
            }

            telemetry.Context.Session.Id = _session;
            telemetry.Context.Device.OperatingSystem = _operatingSystem;
        }
    }
}
