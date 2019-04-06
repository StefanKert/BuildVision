using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.Services;
using BuildVision.Tool.Building;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;
using Microsoft.Build.Framework;
using ErrorItem = BuildVision.Contracts.ErrorItem;

namespace BuildVision.Core
{
    [Export(typeof(IBuildInformationProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BuildInformationProvider : IBuildInformationProvider
    {
        private readonly IPackageSettingsProvider _packageSettingsProvider;
        private readonly IErrorNavigationService _errorNavigationService;
        private readonly IBuildOutputLogger _buildOutputLogger;
        private readonly IStatusBarNotificationService _statusBarNotificationService;
        private readonly IBuildMessagesFactory _buildMessagesFactory;
        private readonly IWindowStateService _windowStateService;
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildService _buildService;
        private readonly ITaskBarInfoService _taskBarInfoService;

        private string _origTextCurrentState;
        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private readonly object _buildProcessLockObject;
        private CancellationTokenSource _buildProcessCancellationToken;
        private int _currentQueuePosOfBuildingProject = 0;

        public IBuildInformationModel BuildInformationModel { get; } = new BuildInformationModel();
        public ObservableCollection<IProjectItem> Projects { get; } = new ObservableCollection<IProjectItem>();

        [ImportingConstructor]
        public BuildInformationProvider(
            [Import(typeof(IBuildOutputLogger))] IBuildOutputLogger buildOutputLogger,
            [Import(typeof(IStatusBarNotificationService))] IStatusBarNotificationService statusBarNotificationService,
            [Import(typeof(IBuildMessagesFactory))] IBuildMessagesFactory buildMessagesFactory,
            [Import(typeof(IWindowStateService))] IWindowStateService windowStateService,
            [Import(typeof(IPackageSettingsProvider))] IPackageSettingsProvider packageSettingsProvider,
            [Import(typeof(IErrorNavigationService))] IErrorNavigationService errorNavigationService,
            [Import(typeof(ISolutionProvider))] ISolutionProvider solutionProvider,
            [Import(typeof(IBuildService))] IBuildService buildService,
            [Import(typeof(ITaskBarInfoService))] ITaskBarInfoService taskBarInfoService)
        {
            _packageSettingsProvider = packageSettingsProvider;
            _errorNavigationService = errorNavigationService;
            _buildOutputLogger = buildOutputLogger;
            _statusBarNotificationService = statusBarNotificationService;
            _buildMessagesFactory = buildMessagesFactory;
            _windowStateService = windowStateService;
            _solutionProvider = solutionProvider;
            _buildService = buildService;
            _taskBarInfoService = taskBarInfoService;
            _buildOutputLogger.OnErrorRaised += BuildOutputLogger_OnErrorRaised;
        }

        public void ReloadCurrentProjects()
        {
            Projects.Clear();
            Projects.AddRange(_solutionProvider.GetProjects());
        }

        public void ResetCurrentProjects()
        {
            Projects.Clear();
        }

        public void ResetBuildInformationModel()
        {
            BuildInformationModel.ResetState();
        }

        private void BuildOutputLogger_OnErrorRaised(BuildProjectContextEntry projectEntry, object e, ErrorLevel errorLevel)
        {
            try
            {
                if (!TryGetProjectItem(projectEntry, out var projectItem))
                {
                    projectEntry.IsInvalid = true;
                    return;
                }

                var errorItem = new ErrorItem(errorLevel);
                switch (errorLevel)
                {
                    case ErrorLevel.Message:
                        errorItem.Init((BuildMessageEventArgs)e);
                        break;

                    case ErrorLevel.Warning:
                        errorItem.Init((BuildWarningEventArgs)e);
                        break;
                    case ErrorLevel.Error:
                        errorItem.Init((BuildErrorEventArgs)e);
                        break;
                    default:
                        break;
                }

                errorItem.VerifyValues();
                AddErrorItem(projectItem, errorItem);

                var args = new BuildErrorRaisedEventArgs(errorLevel, projectItem);
                bool buildNeedCancel = (args.ErrorLevel == ErrorLevel.Error && _packageSettingsProvider.Settings.GeneralSettings.StopBuildAfterFirstError);
                if (buildNeedCancel)
                {
                    _buildService.CancelBuildSolution();
                }

                bool navigateToBuildFailure = (args.ErrorLevel == ErrorLevel.Error && _packageSettingsProvider.Settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnErrorRaised);
                if (navigateToBuildFailure && !ErrorNavigationService.BuildErrorNavigated)
                {
                    _errorNavigationService.NavigateToErrorItem(errorItem);
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private bool TryGetProjectItem(BuildProjectContextEntry projectEntry, out IProjectItem projectItem)
        {
            projectItem = projectEntry.ProjectItem;
            if (projectItem != null)
                return true;

            string projectFile = projectEntry.FileName;
            if (ProjectExtensions.IsProjectHidden(projectFile))
                return false;

            var projectProperties = projectEntry.Properties;
            var project = Projects.FirstOrDefault(x => x.FullName == projectFile);

            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = Projects.First(item => $"{item.FullName}-{item.Configuration}|{item.Platform.Replace(" ", "")}" == $"{projectFile}-{projectConfiguration}|{projectPlatform}");
                if (projectItem == null)
                {
                    TraceManager.Trace(
                        string.Format("Project Item not found by: UniqueName='{0}', Configuration='{1}, Platform='{2}'.",
                            project.UniqueName,
                            projectConfiguration,
                            projectPlatform),
                        EventLogEntryType.Warning);
                    return false;
                }
            }
            else
            {
                return false;
            }

            projectEntry.ProjectItem = projectItem;
            return true;
        }

        private void AddErrorItem(IProjectItem projectItem, ErrorItem errorItem)
        {
            switch (errorItem.Level)
            {
                case ErrorLevel.Message:
                    projectItem.MessagesCount++;
                    break;
                case ErrorLevel.Warning:
                    projectItem.WarningsCount++;
                    break;
                case ErrorLevel.Error:
                    projectItem.ErrorsCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }
            if (errorItem.Level != ErrorLevel.Error)
                return;

            int errorNumber = projectItem.Errors.Count + projectItem.Warnings.Count + projectItem.Messages.Count + 1;
            errorItem.Number = errorNumber;
            switch (errorItem.Level)
            {
                case ErrorLevel.Message:
                    projectItem.Messages.Add(errorItem);
                    break;

                case ErrorLevel.Warning:
                    projectItem.Warnings.Add(errorItem);
                    break;

                case ErrorLevel.Error:
                    projectItem.Errors.Add(errorItem);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }
        }

        private ProjectState GetProjectState(BuildActions buildAction)
        {
            switch (buildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    return ProjectState.Building;

                case BuildActions.BuildActionClean:
                    return ProjectState.Cleaning;

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
        }

        public void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BuildUpdate();

                for (int i = 0; i < BuildInProcessQuantumSleep * BuildInProcessCountOfQuantumSleep; i += BuildInProcessQuantumSleep)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(BuildInProcessQuantumSleep);
                }
            }
        }

        public void BuildStarted(BuildActions buildAction, BuildScopes buildScope)
        {
            _currentQueuePosOfBuildingProject = 0;
            ErrorNavigationService.BuildErrorNavigated = false;
            _buildOutputLogger.Attach();

            ResetBuildInformationModel();
            ResetCurrentProjects();

            BuildInformationModel.BuildStartTime = DateTime.Now;
            BuildInformationModel.BuildFinishTime = null;
            BuildInformationModel.CurrentBuildState = BuildState.InProgress;
            BuildInformationModel.BuildAction = buildAction;
            BuildInformationModel.BuildScope = buildScope;

            _buildProcessCancellationToken = new CancellationTokenSource();
            _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildBegin);
            System.Threading.Tasks.Task.Run(() => Run(_buildProcessCancellationToken.Token), _buildProcessCancellationToken.Token);

            string message = _buildMessagesFactory.GetBuildBeginMajorMessage(BuildInformationModel);
            _statusBarNotificationService.ShowTextWithFreeze(message);
            _origTextCurrentState = message;
            BuildInformationModel.StateMessage = _origTextCurrentState;
            _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, GetFinishedProjectsCount());
        }

        public void ProjectBuildStarted(IProjectItem projectItem, BuildActions buildAction)
        {
            if (BuildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            try
            {
                var projInCollection = Projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForProjectItem(projectItem));
                if (projInCollection == null)
                {
                    Projects.Add(projectItem);
                    projInCollection = projectItem;
                }
                projInCollection.State = GetProjectState(BuildInformationModel.BuildAction);
                projInCollection.BuildFinishTime = null;
                projInCollection.BuildStartTime = DateTime.Now;

                _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, GetFinishedProjectsCount());
                _currentQueuePosOfBuildingProject++;

                if (BuildInformationModel.BuildScope == BuildScopes.BuildScopeSolution &&
                    (BuildInformationModel.BuildAction == BuildActions.BuildActionBuild ||
                     BuildInformationModel.BuildAction == BuildActions.BuildActionRebuildAll))
                {
                    projInCollection.BuildOrder = _currentQueuePosOfBuildingProject;
                }
                BuildInformationModel.CurrentProject = projInCollection;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private int GetFinishedProjectsCount()
        {
            return BuildInformationModel.SucceededProjectsCount + BuildInformationModel.UpToDateProjectsCount + BuildInformationModel.WarnedProjectsCount + BuildInformationModel.FailedProjectsCount;
        }

        public void ProjectBuildFinished(BuildActions buildAction, string projectIdentifier, bool success, bool canceled)
        {
            if (BuildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            var currentProject = Projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == projectIdentifier);
            ProjectState projectState;
            switch (BuildInformationModel.BuildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    if (success)
                    {
                        if (_packageSettingsProvider.Settings.GeneralSettings.ShowWarningSignForBuilds && currentProject.WarningsCount > 0)
                        {
                            projectState = ProjectState.BuildWarning;
                        }
                        else
                        {
                            projectState = _buildOutputLogger.IsProjectUpToDate(currentProject) ? ProjectState.UpToDate : ProjectState.BuildDone;
                            if (projectState == ProjectState.UpToDate)
                            {
                                // do i have to set errorbox here?
                            }
                        }
                    }
                    else
                    {
                        bool buildCancelled = (canceled && currentProject.ErrorsCount == 0);
                        projectState = buildCancelled ? ProjectState.BuildCancelled : ProjectState.BuildError;
                    }
                    break;

                case BuildActions.BuildActionClean:
                    projectState = success ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
            currentProject.Success = success;
            currentProject.State = projectState;
            currentProject.BuildFinishTime = DateTime.Now;

            if (currentProject.State == ProjectState.BuildError && _packageSettingsProvider.Settings.GeneralSettings.StopBuildAfterFirstError)
            {
                _buildService.CancelBuildSolution();
            }

            BuildInformationModel.SucceededProjectsCount = Projects.Count(x => x.State == ProjectState.BuildDone || x.State == ProjectState.CleanDone);
            BuildInformationModel.FailedProjectsCount = Projects.Count(x => x.State == ProjectState.BuildError || x.State == ProjectState.CleanError);
            BuildInformationModel.WarnedProjectsCount = Projects.Count(x => x.State == ProjectState.BuildWarning);
            BuildInformationModel.UpToDateProjectsCount = Projects.Count(x => x.State == ProjectState.UpToDate);
            BuildInformationModel.MessagesCount = Projects.Sum(x => x.MessagesCount);
            BuildInformationModel.ErrorCount = Projects.Sum(x => x.ErrorsCount);
            BuildInformationModel.WarningsCount = Projects.Sum(x => x.WarningsCount);

            if (BuildInformationModel.CurrentProject == null)
            {
                BuildInformationModel.CurrentProject = Projects.Last();
            }

            _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, GetFinishedProjectsCount());
        }

        public void BuildUpdate()
        {
            var message = _origTextCurrentState + _buildMessagesFactory.GetBuildBeginExtraMessage(BuildInformationModel);
            BuildInformationModel.StateMessage = message;
            _statusBarNotificationService.ShowTextWithFreeze(message);
            foreach (var project in Projects)
            {
                project.RaiseBuildElapsedTimeChanged();
            }
        }

        public void BuildFinished(bool success, bool canceled)
        {
            if (BuildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            if (BuildInformationModel.BuildScope == BuildScopes.BuildScopeSolution)
            {
                foreach (var projectItem in Projects)
                {
                    if (projectItem.State == ProjectState.Pending)
                    {
                        projectItem.State = ProjectState.Skipped;
                    }
                }
            }

            _buildProcessCancellationToken.Cancel();

            BuildInformationModel.BuildFinishTime = DateTime.Now;
            if (success)
            {
                if (BuildInformationModel.ErrorCount > 0)
                {
                    BuildInformationModel.CurrentBuildState = BuildState.ErrorDone;
                }
                else
                {
                    BuildInformationModel.CurrentBuildState = BuildState.Done;
                }
            }
            else if (canceled)
                BuildInformationModel.CurrentBuildState = BuildState.Cancelled;
            else
                BuildInformationModel.CurrentBuildState = BuildState.Failed;


            var message = _buildMessagesFactory.GetBuildDoneMessage(BuildInformationModel);
            _statusBarNotificationService.ShowText(message);
            BuildInformationModel.StateMessage = message;
            _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, GetFinishedProjectsCount());

            if (BuildInformationModel.FailedProjectsCount > 0 || canceled)
            {
                _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildError);
            }
            else
            {
                _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildSuccess);
            }


            if (BuildInformationModel.FailedProjectsCount > 0)
            {
                if (_packageSettingsProvider.Settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnBuildDone)
                {
                    foreach (var project in Projects)
                    {
                        if (ErrorNavigationService.BuildErrorNavigated)
                        {
                            break;
                        }

                        foreach (var error in project.Errors)
                        {
                            if (ErrorNavigationService.BuildErrorNavigated)
                                break;
                            _errorNavigationService.NavigateToErrorItem(error);
                        }
                    }
                }
            }
        }
    }
}
