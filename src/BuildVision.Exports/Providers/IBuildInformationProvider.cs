using System.Collections.Generic;
using System.Collections.ObjectModel;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.UI.Models;

namespace BuildVision.Exports.Providers
{
    public interface IBuildInformationProvider
    {
        ObservableCollection<IProjectItem> GetBuildingProjects();
        void ProjectBuildStarted(IProjectItem projectItem, BuildActions buildAction);
        void ProjectBuildFinished(BuildActions buildAction, string projectIdentifier, bool succeess, bool canceled);
        void ReloadCurrentProjects();
        void ResetCurrentProjects();
        void BuildFinished(bool success, bool canceled);
        void BuildStarted(BuildActions buildAction);
        void BuildUpdate();
        void ResetBuildInformationModel();
        IBuildInformationModel GetBuildInformationModel();
    }
}
