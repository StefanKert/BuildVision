using System;
using System.Diagnostics;
using System.Reflection;

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

        public static string GetProductVersion()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            return versionInfo.ProductVersion;
        }
    }
}
