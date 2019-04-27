using System.Collections.Generic;
using System.Collections.ObjectModel;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.UI.Models;

namespace BuildVision.Exports.Providers
{
    public interface IBuildInformationProvider
    {
        IBuildInformationModel BuildInformationModel { get; } 
        ObservableCollection<IProjectItem> Projects { get; }

        void ProjectBuildStarted(IProjectItem projectItem, BuildAction buildAction);
        void ProjectBuildFinished(BuildAction buildAction, string projectIdentifier, bool succeess, bool canceled);
        void ReloadCurrentProjects();
        void ResetCurrentProjects();
        void BuildFinished(bool success, bool canceled);
        void BuildStarted(BuildAction currentBuildAction, BuildScope scope);
        void BuildUpdate();
        void ResetBuildInformationModel();
    }
}
