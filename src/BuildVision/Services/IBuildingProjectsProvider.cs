using System.Collections.ObjectModel;
using BuildVision.UI.Models;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    public interface IBuildingProjectsProvider
    {
        ObservableCollection<ProjectItem> GetBuildingProjects();
        void ProjectBuildFinished(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, bool succeess, bool canceled);
        void ProjectBuildStarted(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction);
        void ReloadCurrentProjects();
    }
}
