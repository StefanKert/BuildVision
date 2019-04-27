using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BuildVision.Common;
using BuildVision.Common.Diagnostics;
using BuildVision.Common.Logging;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Extensions;
using BuildVision.Helpers;
using BuildVision.Services;
using BuildVision.Tool.Building;
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
        private readonly Serilog.ILogger _logger = LogManager.ForContext<BuildInformationProvider>();

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
        private CancellationTokenSource _buildProcessCancellationToken;
        private int _currentQueuePosOfBuildingProject = 0;

        public IBuildInformationModel BuildInformationModel { get; } = new BuildInformationModel();
        public ObservableCollection<IProjectItem> Projects { get; } = new ObservableRangeCollection<IProjectItem>();

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
            ((ObservableRangeCollection< IProjectItem>)Projects).AddRange(_solutionProvider.GetProjects());
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
                projectItem.AddErrorItem(errorItem);

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
                _logger.Error(ex, "Failed to fetch errormessage.");
            }
        }

        private bool TryGetProjectItem(BuildProjectContextEntry projectEntry, out IProjectItem projectItem)
        {
            projectItem = projectEntry.ProjectItem;
            if (projectItem != null)
            {
                return true;
            }

            string projectFile = projectEntry.FileName;
            if (ProjectExtensions.IsProjectHidden(projectFile))
            {
                return false;
            }

            var projectProperties = projectEntry.Properties;
            var project = Projects.FirstOrDefault(x => x.FullName == projectFile);

            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = Projects.First(item => $"{item.FullName}-{item.Configuration}|{item.Platform.Replace(" ", "")}" == $"{projectFile}-{projectConfiguration}|{projectPlatform}");
                if (projectItem == null)
                {
                    _logger.Warning("Project Item not found by: UniqueName='{UniqueName}', Configuration='{ProjectConfiguration}, Platform='{ProjectPlatform}'.", project.UniqueName, projectConfiguration, projectPlatform);
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

                    Thread.Sleep(BuildInProcessQuantumSleep);
                }
            }
        }

        public void BuildStarted(BuildAction buildAction, BuildScope buildScope)
        {
            _currentQueuePosOfBuildingProject = 0;
            ErrorNavigationService.BuildErrorNavigated = false;
            _buildOutputLogger.Attach();

            ResetBuildInformationModel();
            ReloadCurrentProjects();

            BuildInformationModel.BuildStartTime = DateTime.Now;
            BuildInformationModel.BuildFinishTime = null;
            BuildInformationModel.CurrentBuildState = BuildState.InProgress;
            BuildInformationModel.BuildAction = buildAction;
            BuildInformationModel.BuildScope = buildScope;
            BuildInformationModel.BuildId = Guid.NewGuid();

            _buildProcessCancellationToken = new CancellationTokenSource();
            _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildBegin);
            System.Threading.Tasks.Task.Run(() => Run(_buildProcessCancellationToken.Token), _buildProcessCancellationToken.Token);

            string message = _buildMessagesFactory.GetBuildBeginMajorMessage(BuildInformationModel);
            _statusBarNotificationService.ShowTextWithFreeze(message);
            _origTextCurrentState = message;
            BuildInformationModel.StateMessage = _origTextCurrentState;
            _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, GetFinishedProjectsCount());

            DiagnosticsClient.TrackEvent("BuildStarted", new Dictionary<string, string>
            {
                { "BuildId", BuildInformationModel.BuildId.ToString() },
                { "BuildAction", buildAction.ToString() },
                { "BuildScope", buildScope.ToString() }
            });
        }

        public void ProjectBuildStarted(IProjectItem projectItem, BuildAction buildAction)
        {
            if (BuildInformationModel.BuildAction == BuildAction.Deploy)
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
                projInCollection.State = BuildInformationModel.BuildAction.GetProjectState();
                projInCollection.BuildFinishTime = null;
                projInCollection.BuildStartTime = DateTime.Now;

                _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, GetFinishedProjectsCount());
                _currentQueuePosOfBuildingProject++;

                if (BuildInformationModel.BuildScope == BuildScope.Solution &&
                    (BuildInformationModel.BuildAction == BuildAction.Build ||
                     BuildInformationModel.BuildAction == BuildAction.RebuildAll))
                {
                    projInCollection.BuildOrder = _currentQueuePosOfBuildingProject;
                }
                BuildInformationModel.CurrentProject = projInCollection;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed during Project Build start.");
            }
        }

        private int GetFinishedProjectsCount()
        {
            return BuildInformationModel.SucceededProjectsCount + BuildInformationModel.UpToDateProjectsCount + BuildInformationModel.WarnedProjectsCount + BuildInformationModel.FailedProjectsCount;
        }

        public void ProjectBuildFinished(BuildAction buildAction, string projectIdentifier, bool success, bool canceled)
        {
            if (BuildInformationModel.BuildAction == BuildAction.Deploy)
            {
                return;
            }

            var currentProject = Projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == projectIdentifier);
            currentProject.Success = success;
            currentProject.State = GetProjectState(success, canceled, currentProject); 
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

        private ProjectState GetProjectState(bool success, bool canceled, IProjectItem currentProject)
        {
            ProjectState projectState;
            switch (BuildInformationModel.BuildAction)
            {
                case BuildAction.Build:
                case BuildAction.RebuildAll:
                    if (success)
                    {
                        if (currentProject.WarningsCount > 0)
                        {
                            projectState = ProjectState.BuildWarning;
                        }
                        else
                        {
                            projectState = _buildOutputLogger.IsProjectUpToDate(currentProject) ? ProjectState.UpToDate : ProjectState.BuildDone;
                        }
                    }
                    else
                    {
                        bool buildCancelled = (canceled && currentProject.ErrorsCount == 0);
                        projectState = buildCancelled ? ProjectState.BuildCancelled : ProjectState.BuildError;
                    }
                    break;

                case BuildAction.Clean:
                    projectState = success ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(BuildInformationModel.BuildAction));
            }

            return projectState;
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
            if (BuildInformationModel.BuildAction == BuildAction.Deploy)
            {
                return;
            }

            if (BuildInformationModel.BuildScope == BuildScope.Solution)
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
                BuildInformationModel.CurrentBuildState = BuildInformationModel.ErrorCount > 0 ? BuildState.ErrorDone : BuildState.Done;
            }
            else
            {
                BuildInformationModel.CurrentBuildState = canceled ? BuildState.Cancelled : BuildState.Failed;
            }

            DiagnosticsClient.TrackEvent("BuildFinished", new Dictionary<string, string>
            {
                { "BuildId", BuildInformationModel.BuildId.ToString() },
                { "BuildAction", BuildInformationModel.BuildAction.ToString() },
                { "BuildScope", BuildInformationModel.BuildScope.ToString() },
                { "BuildState", BuildInformationModel.CurrentBuildState.ToString() },
                { "BuildStartTime", BuildInformationModel.BuildStartTime.ToString() },
                { "BuildFinishTime", BuildInformationModel.BuildFinishTime.ToString() },
                { "ProjectsCount", Projects.Count.ToString() },
            });

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
                            {
                                break;
                            }

                            _errorNavigationService.NavigateToErrorItem(error);
                        }
                    }
                }
            }
        }
    }
}
