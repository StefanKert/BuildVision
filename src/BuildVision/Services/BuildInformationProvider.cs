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
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell;
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
        private readonly ObservableCollection<IProjectItem> _projects;

        private readonly BuildInformationModel _buildInformationModel;
        private readonly BuildEvents _buildEvents;
        private readonly DTE2 _dte;
        private string _origTextCurrentState;

        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private readonly object _buildProcessLockObject;
        private CancellationTokenSource _buildProcessCancellationToken;
        private int _currentQueuePosOfBuildingProject = 0;


        [ImportingConstructor]
        public BuildInformationProvider(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
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
            _buildInformationModel = new BuildInformationModel();
            _buildEvents = (serviceProvider.GetService(typeof(DTE)) as DTE).Events.BuildEvents;
            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _projects = new ObservableCollection<IProjectItem>();
            _solutionProvider = solutionProvider;
            _buildService = buildService;
            _taskBarInfoService = taskBarInfoService;
            _buildOutputLogger.OnErrorRaised += BuildOutputLogger_OnErrorRaised;
        }

        public IBuildInformationModel GetBuildInformationModel()
        {
            return _buildInformationModel;
        }

        public ObservableCollection<IProjectItem> GetBuildingProjects()
        {
            return _projects;
        }

        public void ReloadCurrentProjects()
        {
            _projects.Clear();
            _projects.AddRange(_solutionProvider.GetProjects());
        }

        public void ResetCurrentProjects()
        {
            _projects.Clear();
        }

        public void ResetBuildInformationModel()
        {
            _buildInformationModel.ErrorCount = 0;
            _buildInformationModel.WarningsCount = 0;
            _buildInformationModel.MessagesCount = 0;
            _buildInformationModel.SucceededProjectsCount = 0;
            _buildInformationModel.UpToDateProjectsCount = 0;
            _buildInformationModel.FailedProjectsCount = 0;
            _buildInformationModel.WarnedProjectsCount = 0;
            _buildInformationModel.StateMessage = Resources.BuildDoneText_BuildNotStarted;
            _buildInformationModel.CurrentBuildState = BuildState.NotStarted;
            _buildInformationModel.BuildAction = BuildActions.Unknown;
            _buildInformationModel.BuildScope = BuildScopes.Unknown;
            _buildInformationModel.BuildStartTime = null;
            _buildInformationModel.BuildFinishTime = null;
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            _buildInformationModel.BuildScope = (BuildScopes)scope; // we need to set buildscope explictly because it is not possible to get this via the other api
            string message = _buildMessagesFactory.GetBuildBeginMajorMessage(_buildInformationModel);
            _statusBarNotificationService.ShowTextWithFreeze(message);
            _origTextCurrentState = message;
            _buildInformationModel.StateMessage = _origTextCurrentState;
            _taskBarInfoService.UpdateTaskBarInfo(_buildInformationModel.CurrentBuildState, _buildInformationModel.BuildScope, _projects.Count, GetFinishedProjectsCount());
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
            var project = _projects.FirstOrDefault(x => x.FullName == projectFile);

            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = _projects.First(item => $"{item.FullName}-{item.Configuration}|{item.Platform.Replace(" ", "")}" == $"{projectFile}-{projectConfiguration}|{projectPlatform}");
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

        public void BuildStarted(BuildActions buildAction)
        {
            _currentQueuePosOfBuildingProject = 0;
            ErrorNavigationService.BuildErrorNavigated = false;
            _buildOutputLogger.Attach();

            ResetBuildInformationModel();
            ResetCurrentProjects();

            _buildInformationModel.BuildStartTime = DateTime.Now;
            _buildInformationModel.BuildFinishTime = null;
            _buildInformationModel.CurrentBuildState = BuildState.InProgress;
            _buildInformationModel.BuildAction = buildAction;

            _buildProcessCancellationToken = new CancellationTokenSource();
            _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildBegin);
            System.Threading.Tasks.Task.Run(() => Run(_buildProcessCancellationToken.Token), _buildProcessCancellationToken.Token);
        }

        public void ProjectBuildStarted(IProjectItem projectItem, BuildActions buildAction)
        {
            if (_buildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            try
            {
                var projInCollection = _projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForProjectItem(projectItem));
                if (projInCollection == null)
                {
                    _projects.Add(projectItem);
                    projInCollection = projectItem;
                }
                projInCollection.State = GetProjectState(_buildInformationModel.BuildAction);
                projInCollection.BuildFinishTime = null;
                projInCollection.BuildStartTime = DateTime.Now;

                _taskBarInfoService.UpdateTaskBarInfo(_buildInformationModel.CurrentBuildState, _buildInformationModel.BuildScope, _projects.Count, GetFinishedProjectsCount());
                _currentQueuePosOfBuildingProject++;

                if (_buildInformationModel.BuildScope == BuildScopes.BuildScopeSolution &&
                    (_buildInformationModel.BuildAction == BuildActions.BuildActionBuild ||
                     _buildInformationModel.BuildAction == BuildActions.BuildActionRebuildAll))
                {
                    projInCollection.BuildOrder = _currentQueuePosOfBuildingProject;
                }
                _buildInformationModel.CurrentProject = projInCollection;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private int GetFinishedProjectsCount()
        {
            return _buildInformationModel.SucceededProjectsCount + _buildInformationModel.UpToDateProjectsCount + _buildInformationModel.WarnedProjectsCount + _buildInformationModel.FailedProjectsCount;
        }

        public void ProjectBuildFinished(BuildActions buildAction, string projectIdentifier, bool success, bool canceled)
        {
            if (_buildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            var currentProject = _projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == projectIdentifier);
            ProjectState projectState;
            switch (_buildInformationModel.BuildAction)
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

            _buildInformationModel.SucceededProjectsCount = _projects.Count(x => x.State == ProjectState.BuildDone || x.State == ProjectState.CleanDone);
            _buildInformationModel.FailedProjectsCount = _projects.Count(x => x.State == ProjectState.BuildError || x.State == ProjectState.CleanError);
            _buildInformationModel.WarnedProjectsCount = _projects.Count(x => x.State == ProjectState.BuildWarning);
            _buildInformationModel.UpToDateProjectsCount = _projects.Count(x => x.State == ProjectState.UpToDate);
            _buildInformationModel.MessagesCount = _projects.Sum(x => x.MessagesCount);
            _buildInformationModel.ErrorCount = _projects.Sum(x => x.ErrorsCount);
            _buildInformationModel.WarningsCount = _projects.Sum(x => x.WarningsCount);

            if (_buildInformationModel.CurrentProject == null)
            {
                _buildInformationModel.CurrentProject = GetBuildingProjects().Last();
            }

            _taskBarInfoService.UpdateTaskBarInfo(_buildInformationModel.CurrentBuildState, _buildInformationModel.BuildScope, _projects.Count, GetFinishedProjectsCount());
        }

        public void BuildUpdate()
        {
            var message = _origTextCurrentState + _buildMessagesFactory.GetBuildBeginExtraMessage(_buildInformationModel);
            _buildInformationModel.StateMessage = message;
            _statusBarNotificationService.ShowTextWithFreeze(message);
            /*
             *             for (int i = 0; i < buildingProjects.Count; i++)
                    buildingProjects[i].RaiseBuildElapsedTimeChanged();
            */
        }

        public void BuildFinished(bool success, bool canceled)
        {
            if (_buildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            if (_buildInformationModel.BuildScope == BuildScopes.BuildScopeSolution)
            {
                foreach (var projectItem in _projects)
                {
                    if (projectItem.State == ProjectState.Pending)
                    {
                        projectItem.State = ProjectState.Skipped;
                    }
                }
            }

            _buildProcessCancellationToken.Cancel();

            _buildInformationModel.BuildFinishTime = DateTime.Now;
            if (success)
            {
                if (_buildInformationModel.ErrorCount > 0)
                {
                    _buildInformationModel.CurrentBuildState = BuildState.ErrorDone;
                }
                else
                {
                    _buildInformationModel.CurrentBuildState = BuildState.Done;
                }
            }
            else if (canceled)
                _buildInformationModel.CurrentBuildState = BuildState.Cancelled;
            else
                _buildInformationModel.CurrentBuildState = BuildState.Failed;


            var message = _buildMessagesFactory.GetBuildDoneMessage(_buildInformationModel);
            _statusBarNotificationService.ShowText(message);
            _buildInformationModel.StateMessage = message;
            _taskBarInfoService.UpdateTaskBarInfo(_buildInformationModel.CurrentBuildState, _buildInformationModel.BuildScope, _projects.Count, GetFinishedProjectsCount());

            if (_buildInformationModel.FailedProjectsCount > 0 || canceled)
            {
                _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildError);
            }
            else
            {
                _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildSuccess);
            }


            if (_buildInformationModel.FailedProjectsCount > 0)
            {
                if (_packageSettingsProvider.Settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnBuildDone)
                {
                    foreach (var project in _projects)
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
