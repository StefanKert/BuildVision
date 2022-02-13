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

        private int _projectsToBuildCount = 0;
        public int PendingProjectsCount
        {
            get => _projectsToBuildCount;
            set => SetProperty(ref _projectsToBuildCount, value);
        }

        private int _builtProjectsCount = 0;
        public int BuiltProjectsCount
        {
            get => _builtProjectsCount;
            set => SetProperty(ref _builtProjectsCount, value);
        }

        private string _stateMessage = Resources.BuildDoneText_BuildNotStarted;
        public string StateMessage
        {
            get => _stateMessage;
            set => SetProperty(ref _stateMessage, value);
        }

        private int _warnedProjectsCount = 0;
        public int WarnedProjectsCount
        {
            get => _warnedProjectsCount;
            set => SetProperty(ref _warnedProjectsCount, value);
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
                OnPropertyChanged(nameof(IsProgressBarVisible));
            }
        }

        public bool IsProgressBarVisible { get { return CurrentBuildState == BuildState.InProgress; } }

        public BuildResultState ResultState => GetBuildResultState();

        private BuildAction _buildAction = BuildAction.Unknown;
        public BuildAction BuildAction
        {
            get => _buildAction;
            set
            {
                SetProperty(ref _buildAction, value);
                OnPropertyChanged(nameof(StateIconKey));
            }
        }

        private BuildScope _buildScope = BuildScope.Unknown;
        public BuildScope BuildScope
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

        private Guid _buildId = Guid.Empty;
        public Guid BuildId
        {
            get => _buildId;
            set => SetProperty(ref _buildId, value);
        }

        private string GetStateIconKey()
        {
            var resultState = GetBuildResultState();
            if (CurrentBuildState == BuildState.InProgress && resultState == BuildResultState.Unknown)
            {
                if (BuildAction == BuildAction.RebuildAll)
                {
                    return "Rebuild";
                }

                if (BuildAction == BuildAction.Clean)
                {
                    return "Clean";
                }

                if (BuildAction == BuildAction.Build)
                {
                    return "Build";
                }
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
                if (BuildAction == BuildAction.RebuildAll)
                {
                    return BuildResultState.RebuildCancelled;
                }

                if (BuildAction == BuildAction.Clean)
                {
                    return BuildResultState.CleanCancelled;
                }

                if (BuildAction == BuildAction.Build)
                {
                    return BuildResultState.BuildCancelled;
                }
                else
                {
                    return BuildResultState.Unknown;
                }
            }
            else if (CurrentBuildState == BuildState.Failed)
            {
                if (BuildAction == BuildAction.RebuildAll)
                {
                    return BuildResultState.RebuildFailed;
                }

                if (BuildAction == BuildAction.Clean)
                {
                    return BuildResultState.CleanFailed;
                }

                if (BuildAction == BuildAction.Build)
                {
                    return BuildResultState.BuildFailed;
                }
                else
                {
                    return BuildResultState.Unknown;
                }
            }
            else if (CurrentBuildState == BuildState.ErrorDone)
            {
                if (BuildAction == BuildAction.RebuildAll)
                {
                    return BuildResultState.RebuildErrorDone;
                }

                if (BuildAction == BuildAction.Clean)
                {
                    return BuildResultState.CleanErrorDone;
                }

                if (BuildAction == BuildAction.Build)
                {
                    return BuildResultState.BuildErrorDone;
                }
                else
                {
                    return BuildResultState.Unknown;
                }
            }
            else if (CurrentBuildState == BuildState.Done)
            {
                if (BuildAction == BuildAction.RebuildAll)
                {
                    return BuildResultState.RebuildDone;
                }

                if (BuildAction == BuildAction.Clean)
                {
                    return BuildResultState.CleanDone;
                }

                if (BuildAction == BuildAction.Build)
                {
                    return BuildResultState.BuildDone;
                }
                else
                {
                    return BuildResultState.Unknown;
                }
            }
            else
            {
                return BuildResultState.Unknown;
            }
        }

        public int GetFinishedProjectsCount() => SucceededProjectsCount + UpToDateProjectsCount + WarnedProjectsCount + FailedProjectsCount;

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
            BuildAction = BuildAction.Unknown;
            BuildScope = BuildScope.Unknown;
            BuildStartTime = null;
            BuildFinishTime = null;
            BuiltProjectsCount = 0;
            PendingProjectsCount = 0;
        }
    }
}
