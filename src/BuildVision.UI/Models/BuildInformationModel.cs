using System;
using System.Windows.Controls;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;

namespace BuildVision.UI.Models
{
    public class BuildInformationModel : BindableBase, IBuildInformationModel
    {
        private int _errorCount = 0;
        public int ErrorCount
        {
            get => _errorCount;
            set => SetProperty(ref _errorCount, value);
        }

        private int _warningsCount = 0;
        public int WarningsCount
        {
            get => _warningsCount;
            set => SetProperty(ref _warningsCount, value);
        }

        private int _messagesCount = 0;
        public int MessagesCount
        {
            get => _messagesCount;
            set => SetProperty(ref _messagesCount, value);
        }

        private int _succeededProjectsCount = 0;
        public int SucceededProjectsCount
        {
            get => _succeededProjectsCount;
            set => SetProperty(ref _succeededProjectsCount, value);
        }

        private int _upToDateProjectsCount;
        public int UpToDateProjectsCount
        {
            get => _upToDateProjectsCount;
            set => SetProperty(ref _upToDateProjectsCount, value);
        }

        private int _failedProjectsCount = 0;
        public int FailedProjectsCount
        {
            get => _failedProjectsCount;
            set => SetProperty(ref _failedProjectsCount, value);
        }

        private int _warnedProjectsCount = 0;
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
            set
            {
                SetProperty(ref _currentBuildState, value);
                OnPropertyChanged(nameof(StateIconKey));
                OnPropertyChanged(nameof(IsFinished));
            }
        }

        public BuildResultState ResultState => GetBuildResultState();

        private BuildActions _buildAction = BuildActions.Unknown;
        public BuildActions BuildAction
        {
            get => _buildAction;
            set
            {
                SetProperty(ref _buildAction, value);
                OnPropertyChanged(nameof(StateIconKey));
            }
        }

        private BuildScopes _buildScope = BuildScopes.Unknown;
        public BuildScopes BuildScope
        {
            get => _buildScope;
            set => SetProperty(ref _buildScope, value);
        }

        private DateTime? _buildStartTime = null;
        public DateTime? BuildStartTime
        {
            get => _buildStartTime;
            set => SetProperty(ref _buildStartTime, value);
        }

        private DateTime? _buildFinishTime = null;
        public DateTime? BuildFinishTime
        {
            get => _buildFinishTime;
            set => SetProperty(ref _buildFinishTime, value);
        }

        public string StateIconKey
        {
            get => GetStateIconKey();
        }

        public bool IsFinished
        {
            get => CurrentBuildState > BuildState.InProgress;
        }

        private IProjectItem _currentProject = null;
        public IProjectItem CurrentProject
        {
            get => _currentProject;
            set => SetProperty(ref _currentProject, value);
        }
   
        private string GetStateIconKey()
        {
            var resultState = GetBuildResultState();
            if (CurrentBuildState == BuildState.InProgress && resultState == BuildResultState.Unknown)
            {
                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return "Rebuild";
                if (BuildAction == BuildActions.BuildActionClean)
                    return "Clean";
                if (BuildAction == BuildActions.BuildActionBuild)
                    return "Build";
            }

            if (resultState == BuildResultState.Unknown)
            {
                return "StandBy";
            }
            return resultState.ToString();            
        }

        private BuildResultState GetBuildResultState()
        {
            if (CurrentBuildState == BuildState.InProgress)
            {
                return BuildResultState.Unknown;
            }
            else if (CurrentBuildState == BuildState.Cancelled)
            {
                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildCancelled;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanCancelled;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildCancelled;
                else
                    return BuildResultState.Unknown;
            }
            else if (CurrentBuildState == BuildState.Failed)
            {
                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildFailed;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanFailed;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildFailed;
                else
                    return BuildResultState.Unknown;
            }
            else if (CurrentBuildState == BuildState.ErrorDone)
            {
                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildErrorDone;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanErrorDone;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildErrorDone;
                else
                    return BuildResultState.Unknown;
            }
            else if (CurrentBuildState == BuildState.Done)
            {
                if (BuildAction == BuildActions.BuildActionRebuildAll)
                    return BuildResultState.RebuildDone;
                if (BuildAction == BuildActions.BuildActionClean)
                    return BuildResultState.CleanDone;
                if (BuildAction == BuildActions.BuildActionBuild)
                    return BuildResultState.BuildDone;
                else
                    return BuildResultState.Unknown;
            }
            else
            {
                return BuildResultState.Unknown;
            }
        }

        public void ResetState()
        {
            ErrorCount = 0;
            WarningsCount = 0;
            MessagesCount = 0;
            SucceededProjectsCount = 0;
            UpToDateProjectsCount = 0;
            FailedProjectsCount = 0;
            WarnedProjectsCount = 0;
            StateMessage = Resources.BuildDoneText_BuildNotStarted;
            CurrentBuildState = BuildState.NotStarted;
            BuildAction = BuildActions.Unknown;
            BuildScope = BuildScopes.Unknown;
            BuildStartTime = null;
            BuildFinishTime = null;
        }
    }
}
