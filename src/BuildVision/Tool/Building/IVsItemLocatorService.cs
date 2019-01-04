using BuildVision.Contracts;
using BuildVision.UI;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.Tool.Building
{
    public interface IVsItemLocatorService
    {
        ProjectItem AddProjectToVisibleProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName);
        ProjectItem AddProjectToVisibleProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName, string configuration, string platform);
        ProjectItem FindProjectItemInProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName);
        ProjectItem FindProjectItemInProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName, string configuration, string platform);

        ProjectItem GetCurrentProject(IBuildVisionPaneViewModel viewModel, BuildScopes? buildScope, string project, string projectconfig, string platform);
        bool GetProjectItem(IBuildVisionPaneViewModel viewModel, BuildScopes? buildScope, BuildProjectContextEntry projectEntry, out ProjectItem projectItem);
    }
}
