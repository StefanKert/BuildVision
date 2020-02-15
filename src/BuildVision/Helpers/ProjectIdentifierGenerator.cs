using BuildVision.UI.Models;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Helpers
{
    public static class ProjectIdentifierGenerator
    {
        public static string GetIdentifierForInteropTypes(IVsHierarchy pHierProj, IVsCfg pCfgProj)
        {
            var project = pHierProj.ToProject();
            pCfgProj.get_DisplayName(out var projConfiguration);
            return $"{project.UniqueName}-{projConfiguration}";
        }

        public static string GetIdentifierForProjectItem(IProjectItem projectItem)
        {
            return $"{projectItem.UniqueName}-{projectItem.Configuration}|{projectItem.Platform}";
        }
    }
}
