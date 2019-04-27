using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BuildVision.Common
{
    public class AppVersionInfo
    {
        public string AppVersion { get; set; }
        public string BuildVersion { get; set; }

        public AppVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            AppVersion = versionInfo.ProductVersion.ToString();
            BuildVersion = new Version(versionInfo.FileVersion).ToString();
        }
    }
}
