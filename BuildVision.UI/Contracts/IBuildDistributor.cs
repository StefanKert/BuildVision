using BuildVision.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
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
