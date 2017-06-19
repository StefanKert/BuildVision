using System;

namespace BuildVision.Contracts
{
    public class BuildErrorRaisedEventArgs : EventArgs
    {
        public ErrorLevel ErrorLevel { get; private set; }
        public BuildedProject ProjectInfo { get; private set; }

        public BuildErrorRaisedEventArgs(ErrorLevel errorLevel, BuildedProject projectInfo)
        {
            ErrorLevel = errorLevel;
            ProjectInfo = projectInfo;
        }
    }
}