using BuildVision.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BuildVision.Helpers
{
    public class GithubHelper
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

            var url = GetUrlForNewBug(string.Format(template, VsVersion.FullVersion, appVersion.BuildVersion, Environment.OSVersion));
            Process.Start(new ProcessStartInfo(url));
        }

        public static string GetUrlForNewBug(string body)
        {
            return string.Format(URL_TEMPLATE, "Bug", "", ASSIGNEE, WebUtility.UrlEncode(body));
        }
    }
}
