using BuildVision.Contracts;

namespace BuildVision.Exports.Services
{
    public interface ITaskBarInfoService
    {
        void ResetTaskBarInfo(bool ifTaskBarProgressEnabled = true);
        void UpdateTaskBarInfo(BuildState buildState, BuildScope buildScope, int projectsCount, int finishedProjects);
    }
}
