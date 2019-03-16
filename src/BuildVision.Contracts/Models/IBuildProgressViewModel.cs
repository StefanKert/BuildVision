namespace BuildVision.Contracts.Models
{
    public interface IBuildProgressViewModel
    {
        bool ActionProgressIsPaused { get; set; }
        bool ActionProgressIsVisible { get; set; }
        int CurrentQueuePosOfBuildingProject { get; }

        void OnBuildBegin(int projectsCount);
        void OnBuildCancelled();
        void OnBuildDone();
        void OnBuildProjectBegin();
        void OnBuildProjectDone(bool success);
        void ResetTaskBarInfo(bool ifTaskBarProgressEnabled = true);
    }
}
