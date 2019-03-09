namespace BuildVision.Exports.Services
{
    public interface IBuildService
    {
        void ShowGridColumnsSettingsPage();
        void ShowGeneralSettingsPage();
        void BuildSolution();
        void CleanSolution();
        void RebuildSolution();
        void CancelBuildSolution();
        void ProjectCopyBuildOutputFilesToClipBoard();
        void RaiseCommandForSelectedProject();
    }
}
