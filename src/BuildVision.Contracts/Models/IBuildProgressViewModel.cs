namespace BuildVision.Contracts.Models
{
    public interface IBuildProgressViewModel
    {
        bool ActionProgressIsPaused { get; set; }
        int CurrentQueuePosOfBuildingProject { get; }

        void OnBuildBegin(IBuildInformationModel buildInformationModel, int projectsCount);
        void OnBuildCancelled();
        void OnBuildDone();
        void OnBuildProjectBegin();
        void OnBuildProjectDone(bool success);
        void ResetTaskBarInfo(bool ifTaskBarProgressEnabled = true);
    }
}
