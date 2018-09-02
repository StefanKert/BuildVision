using System;
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
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            Version version = assemblyName.Version;

            AppVersion = version.ToString(3);
            BuildVersion = version.ToString(4);
            BuildDateTime = RetrieveLinkerTimestamp(assembly);
        }

        /// <summary>
        /// Get last build datetime.
        /// </summary>
        private static DateTime RetrieveLinkerTimestamp(Assembly assembly)
        {
            string filePath = assembly.Location;
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
                    stream.Close();
            }

            int i = BitConverter.ToInt32(buffer, PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(buffer, i + LinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}
