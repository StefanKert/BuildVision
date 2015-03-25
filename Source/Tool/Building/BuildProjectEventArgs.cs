using System;
using AlekseyNagovitsyn.BuildVision.Tool.Models;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class BuildProjectEventArgs : EventArgs
    {
        public ProjectItem ProjectItem { get; private set; }
        public ProjectState ProjectState { get; private set; }
        public DateTime EventTime { get; private set; }
        public BuildedProject BuildedProjectInfo { get; private set; }

        public BuildProjectEventArgs(
            ProjectItem projectItem, 
            ProjectState projectState, 
            DateTime eventTime, 
            BuildedProject buildedProjectInfo)
        {
            ProjectItem = projectItem;
            ProjectState = projectState;
            EventTime = eventTime;
            BuildedProjectInfo = buildedProjectInfo;
        }
    }
}