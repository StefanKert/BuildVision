using BuildVision.UI.Models;

namespace BuildVision.Exports.Services
{
    public interface IBuildService
    {
        void BuildSolution();
        void CleanSolution();
        void RebuildSolution();
        void CancelBuildSolution();
        void ProjectCopyBuildOutputFilesToClipBoard(IProjectItem projItem);
        void RaiseCommandForSelectedProject(IProjectItem projectItem, int command);
    }
}
