using System;
using System.Diagnostics;

namespace BuildVision.Common
{
    public static class ApplicationInfo
    {
        public const string ApplicationName = "BuildVision";

        public static FileVersionInfo GetHostVersionInfo()
        {
            return Process.GetCurrentProcess().MainModule.FileVersionInfo;
        }

        public static string GetPackageVersion(object package)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(package.GetType().Assembly.Location);
            return versionInfo.ProductVersion;
        }
    }
}
