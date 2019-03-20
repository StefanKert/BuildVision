using System;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;

namespace BuildVision.UI.Models
{
    public class BuildInformationModel : BindableBase, IBuildInformationModel
    {
        private int _errorCount;
        public int ErrorCount
        {
            get => _errorCount;
            set => SetProperty(ref _errorCount, value);
        }

        private int _warningsCount;
        public int WarningsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _warningsCount, value);
        }

        private int _messagesCount;
        public int MessagesCount
        {
            get => _messagesCount;
            set => SetProperty(ref _messagesCount, value);
        }

        private int _succeededProjectsCount;
        public int SucceededProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _succeededProjectsCount, value);
        }

        private int _upToDateProjectsCount;
        public int UpToDateProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _upToDateProjectsCount, value);
        }

        private int _failedProjectsCount;
        public int FailedProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _failedProjectsCount, value);
        }

        private int _warnedProjectsCount;
        public int WarnedProjectsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _warnedProjectsCount, value);
        }

        private string _stateMessage = Resources.BuildDoneText_BuildNotStarted;
        public string StateMessage
        {
            get => _stateMessage;
            set => SetProperty(ref _stateMessage, value);
        }

        private BuildState _currentBuildState = BuildState.NotStarted;
        public BuildState CurrentBuildState
        {
            get => _currentBuildState;
            set => SetProperty(ref _currentBuildState, value);
        }

        public BuildResultState ResultState => GetBuildState();

        private BuildActions _buildAction = BuildActions.Unknown;
        public BuildActions BuildAction
        {
            get => _buildAction;
            set => SetProperty(ref _buildAction, value);
        }

        private BuildScopes _buildScope = BuildScopes.Unknown;
        public BuildScopes BuildScope
        {
            get => _buildScope;
            set => SetProperty(ref _buildScope, value);
        }

        private DateTime? _buildStartTime;
        public DateTime? BuildStartTime
        {
            get => _buildStartTime;
            set => SetProperty(ref _buildStartTime, value);
        }

        private DateTime? _buildFinishTime;
        public DateTime? BuildFinishTime
        {
            get => _buildFinishTime;
            set => SetProperty(ref _buildFinishTime, value);
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
}
