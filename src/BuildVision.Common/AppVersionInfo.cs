using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BuildVision.Common
{
    public class AppVersionInfo
    {
        public string AppVersion { get; set; }
        public string BuildVersion { get; set; }
        public DateTime BuildDateTime { get; set; }

        public AppVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            AppVersion = new Version(versionInfo.ProductVersion).ToString();
            BuildVersion = new Version(versionInfo.FileVersion).ToString();
            BuildDateTime = RetrieveLinkerTimestamp(assembly);
        }

        /// <summary>
        /// Get last build datetime.
        /// </summary>
        private static DateTime RetrieveLinkerTimestamp(Assembly assembly)
        {
            var filePath = assembly.Location;
            const int PeHeaderOffset = 60;
            const int LinkerTimestampOffset = 8;
            var buffer = new byte[2048];
            Stream stream = null;

            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                stream.Read(buffer, 0, 2048);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            var i = BitConverter.ToInt32(buffer, PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, i + LinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}
