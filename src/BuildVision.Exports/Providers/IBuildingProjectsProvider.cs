using System.Collections.ObjectModel;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.Exports.Providers
{
    public interface IBuildingProjectsProvider
    {
        ObservableCollection<IProjectItem> GetBuildingProjects();
        void ProjectBuildStarted(IProjectItem projectItem, uint dwAction);
        void ProjectBuildFinished(string projectIdentifier, bool succeess, bool canceled);
        void ReloadCurrentProjects();
        void ResetCurrentProjects();
        bool TryGetProjectItem(BuildProjectContextEntry projectEntry, out IProjectItem projectItem);
    }
}
