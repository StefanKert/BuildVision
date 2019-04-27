using System.Windows.Shell;
using BuildVision.Contracts;

namespace BuildVision.Extensions
{
    public static class BuildStateExtensions
    {
        public static TaskbarItemProgressState ToTaskBarItemProgressState(this BuildState buildState, BuildScope buildScope)
        {
            var progressState = TaskbarItemProgressState.Normal;
            if (buildState == BuildState.Cancelled)
            {
                progressState = TaskbarItemProgressState.Paused;
            }
            else if (buildState == BuildState.Failed)
            {
                progressState = TaskbarItemProgressState.Error;
            }
            else if (buildState != BuildState.InProgress)
            {
                progressState = TaskbarItemProgressState.None;
            }
            else if (buildScope != BuildScope.Solution)
            {
                progressState = TaskbarItemProgressState.Indeterminate;
            }
            else
            {
                progressState = TaskbarItemProgressState.Normal;
            }

            return progressState;
        }
    }
}
