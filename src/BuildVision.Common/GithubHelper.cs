using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using BuildVision.Common;

namespace BuildVision.Helpers
{
    public static class GithubHelper
    {
        const string ASSIGNEE = "stefankert";
        const string URL_TEMPLATE = "https://github.com/StefanKert/BuildVision/issues/new?labels={0}&title={1}&assignee={2}&body={3}";
        const string template = @"
- Visual Studio Version: {0}
- BuildVision Version: {1} 
- OS Version: {2}

Steps to Reproduce:

1.
2.
";

        public static void OpenBrowserWithPrefilledIssue()
        {
            var appVersion = new AppVersionInfo();

            var url = GetUrlForNewBug(string.Format(CultureInfo.CurrentCulture, template, VsVersion.FullVersion, appVersion.BuildVersion, Environment.OSVersion));
            Process.Start(new ProcessStartInfo(url.ToString()));
        }

        public static Uri GetUrlForNewBug(string body)
        {
            return new Uri(string.Format(CultureInfo.CurrentCulture, URL_TEMPLATE, "Bug", "", ASSIGNEE, WebUtility.UrlEncode(body)));
        }
    }
}
