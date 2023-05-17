using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
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
        private const int BuildInProcessQuantumSleep = 250;
        private int _currentQueuePosOfBuildingProject = 0;
        private Timer _timer;

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
            BuildLoggerProvider.OnErrorRaised += BuildOutputLogger_OnErrorRaised;
        }

        public void ReloadCurrentProjects()
        {
            Projects.Clear();
            ((ObservableRangeCollection<IProjectItem>)Projects).AddRange(_solutionProvider.GetProjects());
        }

        public void ResetCurrentProjects() => Projects.Clear();

        public void ResetBuildInformationModel() => BuildInformationModel.ResetState();

        private void BuildOutputLogger_OnErrorRaised(BuildProjectContextEntry projectEntry, object e, ErrorLevel errorLevel)
        {
            try
            {
                var projectItem = projectEntry.ProjectItem;
                if (projectItem == null)
                {
                    if (!TryGetProjectItem(projectEntry, out projectItem))
                    {
                        projectEntry.IsInvalid = true;
                        return;
                    }
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
            projectItem = projectEntry?.ProjectItem;
            if (projectItem != null)
            {
                return true;
            }

            string projectFile = projectEntry.ProjectFile;
            if (ProjectExtensions.IsProjectHidden(projectFile))
            {
                return false;
            }

            var projectProperties = projectEntry.Properties;
            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = Projects.FirstOrDefault(item => $"{item.FullName}-{item.Configuration}|{item.Platform.Replace(" ", "")}" == $"{projectFile}-{projectConfiguration}|{projectPlatform}");
                if (projectItem == null)
                {
                    var project = Projects.FirstOrDefault(x => x.FullName == projectFile);
                    if (project == null)
                    {
                        _logger.Error("Project Item and Project were not found by: ProjectFile='{ProjectFile}', Configuration='{ProjectConfiguration}, Platform='{ProjectPlatform}'.", projectFile, projectConfiguration, projectPlatform);
                    }
                    else
                    {
                        _logger.Warning("Project Item not found by: UniqueName='{UniqueName}', Configuration='{ProjectConfiguration}, Platform='{ProjectPlatform}'.", project.UniqueName, projectConfiguration, projectPlatform);
                    }
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

        public void BuildStarted(BuildAction buildAction, BuildScope buildScope)
        {
            _currentQueuePosOfBuildingProject = 0;
            ErrorNavigationService.BuildErrorNavigated = false;
            //_buildOutputLogger.Attach();

            ResetBuildInformationModel();
            ReloadCurrentProjects();

            BuildInformationModel.BuildStartTime = DateTime.Now;
            BuildInformationModel.BuildFinishTime = null;
            BuildInformationModel.CurrentBuildState = BuildState.InProgress;
            BuildInformationModel.BuildAction = buildAction;
            BuildInformationModel.BuildScope = buildScope;
            BuildInformationModel.BuildId = Guid.NewGuid();
            BuildInformationModel.BuiltProjectsCount = 0;
            BuildInformationModel.PendingProjectsCount = Projects.Count(x => x.State == ProjectState.Pending);

            _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildBegin);
            _timer = new Timer(state => BuildUpdate(), null, BuildInProcessQuantumSleep, Timeout.Infinite);

            string message = _buildMessagesFactory.GetBuildBeginMajorMessage(BuildInformationModel);
            _statusBarNotificationService.ShowTextWithFreeze(message);
            _origTextCurrentState = message;
            BuildInformationModel.StateMessage = _origTextCurrentState;
            UpdateTaskBar();
            BuildStateChanged();

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

            var projInCollection = Projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForProjectItem(projectItem));
            if (projInCollection == null)
            {
                Projects.Add(projectItem);
                projInCollection = projectItem;
            }
            projInCollection.State = BuildInformationModel.BuildAction.GetProjectState();
            projInCollection.BuildFinishTime = null;
            projInCollection.BuildStartTime = DateTime.Now;

            UpdateTaskBar();

            _currentQueuePosOfBuildingProject++;

            if (BuildInformationModel.BuildScope == BuildScope.Solution &&
                (BuildInformationModel.BuildAction == BuildAction.Build ||
                 BuildInformationModel.BuildAction == BuildAction.RebuildAll))
            {
                projInCollection.BuildOrder = _currentQueuePosOfBuildingProject;
            }
            BuildInformationModel.CurrentProject = projInCollection;
            BuildStateChanged();
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
            BuildInformationModel.BuiltProjectsCount++;

            if (BuildInformationModel.CurrentProject == null)
            {
                BuildInformationModel.CurrentProject = Projects.Last();
            }

            UpdateTaskBar();
            BuildStateChanged();
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
                            projectState = (BuildLoggerProvider.BuildOutputLogger?.IsProjectUpToDate(currentProject) ?? false) ?
                                           ProjectState.UpToDate :
                                           ProjectState.BuildDone;
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

        private void BuildUpdate()
        {
            try
            {
                if(BuildInformationModel.CurrentBuildState != BuildState.InProgress)
                {
                    _logger.Error("Build is finished but thread still running.");
                }
                var message = _origTextCurrentState + _buildMessagesFactory.GetBuildBeginExtraMessage(BuildInformationModel);
                BuildInformationModel.StateMessage = message;
                _statusBarNotificationService.ShowTextWithFreeze(message);
                foreach (var project in Projects.Where(x => x.State != ProjectState.Cleaning && x.State != ProjectState.Building))
                {
                    project.RaiseBuildElapsedTimeChanged();
                }
            }
            finally
            {
                try
                {
                    // Only now fire off next event to avoid overlapping timers
                    _timer.Change(BuildInProcessQuantumSleep, Timeout.Infinite);
                }
                catch (ObjectDisposedException)
                {   // Can be disposed underneath us
                }
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

            _timer.Dispose();
            _logger.Information("Canceled build");
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
            UpdateTaskBar();
            BuildStateChanged();
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

        private void UpdateTaskBar()
        {
            _taskBarInfoService.UpdateTaskBarInfo(BuildInformationModel.CurrentBuildState, BuildInformationModel.BuildScope, Projects.Count, BuildInformationModel.GetFinishedProjectsCount());
        }

        public event Action BuildStateChanged;
    }
}
