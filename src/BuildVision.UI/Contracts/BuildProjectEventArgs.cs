using System;
using BuildVision.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.UI.Contracts
{
    public class BuildProjectEventArgs : EventArgs
    {
        public ProjectItem ProjectItem { get; private set; }
        public ProjectState ProjectState { get; private set; }
        public DateTime EventTime { get; private set; }
        public BuildedProject BuildedProjectInfo { get; private set; }

        public BuildProjectEventArgs(ProjectItem projectItem, ProjectState projectState, DateTime eventTime, BuildedProject buildedProjectInfo)
        {
            ProjectItem = projectItem;
            ProjectState = projectState;
            EventTime = eventTime;
            BuildedProjectInfo = buildedProjectInfo;
        }
    }
}