using System;
using System.Diagnostics;
using System.IO;

namespace BuildVision.Helpers
{
    public static class VsVersion
    {
        static readonly object mLock = new object();
        static Version mVsVersion;

        public static Version FullVersion
        {
            get
            {
                lock (mLock)
                {
                    if (mVsVersion == null)
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                        if (File.Exists(path))
                        {
                            var fvi = FileVersionInfo.GetVersionInfo(path);

                            var verName = fvi.ProductVersion;

                            for (var i = 0; i < verName.Length; i++)
                            {
                                if (!char.IsDigit(verName, i) && verName[i] != '.')
                                {
                                    verName = verName.Substring(0, i);
                                    break;
                                }
                            }
                            mVsVersion = new Version(verName);
                        }
                        else
                        {
                            mVsVersion = new Version(0, 0); // Not running inside Visual Studio!
                        }
                    }
                }

                return mVsVersion;
            }
        }
    }
}
