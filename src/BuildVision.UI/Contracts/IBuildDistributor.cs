using BuildVision.Contracts;
using System;

namespace BuildVision.UI.Contracts
{
    public interface IBuildDistributor
    {
        event EventHandler OnBuildBegin;
        event EventHandler OnBuildProcess;
        event EventHandler OnBuildDone;
        event EventHandler OnBuildCancelled;
        event EventHandler<BuildProjectEventArgs> OnBuildProjectBegin;
        event EventHandler<BuildProjectEventArgs> OnBuildProjectDone;
        event EventHandler<BuildErrorRaisedEventArgs> OnErrorRaised;

        void CancelBuild();
    }
}
