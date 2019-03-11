using System;
using BuildVision.UI.Models;

namespace BuildVision.Contracts
{
    public class BuildErrorRaisedEventArgs : EventArgs
    {
        public ErrorLevel ErrorLevel { get; private set; }
        public IProjectItem ProjectInfo { get; private set; }

        public BuildErrorRaisedEventArgs(ErrorLevel errorLevel, IProjectItem projectInfo)
        {
            ErrorLevel = errorLevel;
            ProjectInfo = projectInfo;
        }
    }
}
