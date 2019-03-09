using System;
using System.Collections.ObjectModel;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;

namespace BuildVision.Core
{

    public class VisualStudioSolution : BindableBase
    {
        public BuildState CurrentBuildState { get; set; }
        public BuildResultState ResultState => GetBuildState();
        public BuildActions BuildAction { get; set; }
        public BuildScopes BuildScope { get; set; }
        public DateTime? BuildStartTime { get; set; }
        public DateTime? BuildFinishTime { get; set; }

        public ObservableCollection<ProjectItem> Projects { get; }
 
        public VisualStudioSolution()
        {
            Projects = new ObservableCollection<ProjectItem>();
        }

        public BuildResultState GetBuildState()
        {
            if (CurrentBuildState == BuildState.InProgress)
            {
                return BuildResultState.Unknown;
            }
            else if (CurrentBuildState == BuildState.Done)
            {
                //TODO Decied what happened
                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildCancelled;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanCancelled;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildCancelled;
                else
                    return BuildResultState.Unknown;

                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildFailed;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanFailed;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildFailed;
                else
                    return BuildResultState.Unknown;

                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildSucceeded;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanSucceeded;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildSucceeded;
                else
                    return BuildResultState.Unknown;

                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildSucceededWithErrors;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanSucceededWithErrors;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildSucceededWithErrors;
                else
                    return BuildResultState.Unknown;
            }
            else
            {
                return BuildResultState.Unknown;
            }
        }
    }


    /*
     * General
     * - BuildState / ResultState
     * - StateMessage
     * - ErrorsCount
     * - WarningsCount
     * - InformationCount
     * - SucceededProjects
     * - UpToDateProjects
     * - WarningProjects
     * - FailedProjects
     * 
     * Projects
     * 
     * Actions
     * - Build Solution
     * - Rebuild Solution
     * - Clean Solution
     * - Cancel
     * * 
     * */
}
