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

        public static Version GetPackageVersion(object package)
        {
            return package.GetType().Assembly.GetName().Version;
        }
    }
}
