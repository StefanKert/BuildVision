using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Helpers
{
    public static class VsCfgExtensions
    {
        public static Tuple<string, string> ToConfigurationTuple(this IVsCfg pCfgProj)
        {
            pCfgProj.get_DisplayName(out var projConfiguration);
            var config = projConfiguration.Split('|');

            return new Tuple<string, string>(config[0], config[1]);
        }
    }
}
