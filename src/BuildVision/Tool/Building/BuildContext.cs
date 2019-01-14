using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Core;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Extensions;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Settings.Models.ToolWindow;
using BuildVision.UI.ViewModels;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorItem = BuildVision.Contracts.ErrorItem;
using ProjectItem = BuildVision.UI.Models.ProjectItem;
using WindowState = BuildVision.UI.Models.WindowState;

namespace BuildVision.Tool.Building
{
    public class BuildContext : IBuildInfo, IBuildDistributor
    { 
        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private const string CancelBuildCommand = "Build.Cancel";

        private readonly object _buildingProjectsLockObject;
        private readonly object _buildProcessLockObject = new object();
        private readonly IVsItemLocatorService _locatorService;
        private readonly IStatusBarNotificationService _statusBarNotificationService;
        private readonly IPackageContext _packageContext;
        private readonly DTE _dte;
        private readonly Solution _solution;
        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        private BuildOutputLogger _buildLogger;
        private CancellationTokenSource _buildProcessCancellationToken;
        private bool _buildCancelledInternally;
        private readonly BuildVisionPaneViewModel _viewModel;
        private bool _buildCancelled;
        private readonly BuildEvents _buildEvents;
        private readonly CommandEvents _commandEvents;
        private readonly ToolWindowManager _toolWindowManager;
        private bool _buildErrorIsNavigated;
        private string _origTextCurrentState;

        public bool BuildIsCancelled => _buildCancelled && !_buildCancelledInternally;

        public BuildActions? BuildAction { get; private set; }

        public BuildScopes? BuildScope { get; private set; }

        public BuildState CurrentBuildState { get; private set; }

        public DateTime? BuildStartTime { get; private set; }

        public DateTime? BuildFinishTime { get; private set; }

        public BuildedProjectsCollection BuildedProjects { get; }

        public IList<ProjectItem> BuildingProjects { get; }

        public BuildedSolution BuildedSolution { get; private set; }

        public BuildedSolution BuildingSolution { get; private set; }

        public ProjectItem BuildScopeProject { get; private set; }

        public BuildContext(IVsItemLocatorService locatorService, IStatusBarNotificationService statusBarNotificationService, IPackageContext packageContext, DTE dte, BuildVisionPaneViewModel viewModel)
        {
            _viewModel = viewModel;
            BuildedProjects = new BuildedProjectsCollection();
            BuildingProjects = new List<ProjectItem>();
            _buildingProjectsLockObject = ((ICollection)BuildingProjects).SyncRoot;
            _locatorService = locatorService;
            _statusBarNotificationService = statusBarNotificationService;
            _packageContext = packageContext;
            _dte = dte;
            _buildEvents = dte.Events.BuildEvents;
            _commandEvents = dte.Events.CommandEvents;

            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildDone += (s, e) => BuildEvents_OnBuildDone();
            _buildEvents.OnBuildProjConfigBegin += BuildEvents_OnBuildProjectBegin;
            _buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjectDone;

            _commandEvents.AfterExecute += CommandEvents_AfterExecute;
        }

        public void OverrideBuildProperties(BuildActions? buildAction = null, BuildScopes? buildScope = null)
        {
            BuildAction = buildAction ?? BuildAction;
            BuildScope = buildScope ?? BuildScope;
        }

        public async Task CancelBuildAsync()
        {
            if(BuildAction == BuildActions.BuildActionClean)
                return;
            if (CurrentBuildState != BuildState.InProgress || _buildCancelled || _buildCancelledInternally)
                return;

            try
            {
                // We need to create a separate task here because of some weird things that are going on
                // when calling ExecuteCommand directly. Directly calling it leads to a freeze. No need 
                // for that!
                await _packageContext.ExecuteCommandAsync(CancelBuildCommand);
                _buildCancelledInternally = true;
            }
            catch (Exception ex)
            {
                ex.Trace("Cancel build failed.");
            }
        }

        private void RegisterLogger()
        {
            _buildLogger = null;

            // Same Verbosity as in the Error List.
            const LoggerVerbosity LoggerVerbosity = LoggerVerbosity.Quiet;
            RegisterLoggerResult result = BuildOutputLogger.Register(_parsingErrorsLoggerId, LoggerVerbosity, out _buildLogger);

            if (result == RegisterLoggerResult.AlreadyExists)
            {
                _buildLogger.Projects?.Clear();
            }
        }

        private void CommandEvents_AfterExecute(string guid, int id, object customIn, object customOut)
        {
            if (id == (int)VSConstants.VSStd97CmdID.CancelBuild
                && Guid.Parse(guid) == VSConstants.GUID_VSStandardCommandSet97)
            {
                _buildCancelled = true;
                if (!_buildCancelledInternally)
                    OnBuildCancelled();
            }
        }

        private ProjectState GetProjectState()
        {
            ProjectState projectState;
            switch (BuildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    projectState = ProjectState.Building;
                    break;

                case BuildActions.BuildActionClean:
                    projectState = ProjectState.Cleaning;
                    break;

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(BuildAction));
            }

            return projectState;
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (action == vsBuildAction.vsBuildActionDeploy)
                return;

            RegisterLogger();

            CurrentBuildState = BuildState.InProgress;

            BuildStartTime = DateTime.Now;
            BuildFinishTime = null;
            BuildAction = (BuildActions) action;

            switch (scope)
            {
                case vsBuildScope.vsBuildScopeSolution:
                case vsBuildScope.vsBuildScopeBatch:
                case vsBuildScope.vsBuildScopeProject:
                    BuildScope = (BuildScopes) scope;
                    break;

                case 0:
                    // Scope may be 0 in case of Clean solution, then Start (F5).
                    BuildScope = (BuildScopes) vsBuildScope.vsBuildScopeSolution;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("scope");
            }

            if (scope == vsBuildScope.vsBuildScopeProject)
            {
                var selectedProjects = _dte.ActiveSolutionProjects as object[];
                if (selectedProjects?.Length == 1)
                {
                    var projectItem = new ProjectItem();
                    ViewModelHelper.UpdateProperties((Project) selectedProjects[0], projectItem);
                    BuildScopeProject = projectItem;
                }
            }

            _buildCancelled = false;
            _buildCancelledInternally = false;

            BuildedProjects.Clear();
            lock (_buildingProjectsLockObject)
            {
                BuildingProjects.Clear();
            }

            BuildedSolution = null;
            var solution = _dte.Solution;
            BuildingSolution = new BuildedSolution(solution.FullName, solution.FileName);

            OnBuildBegin();

            _buildProcessCancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(BuildEvents_BuildInProcess, _buildProcessCancellationToken.Token, _buildProcessCancellationToken.Token);
        }

        private void BuildEvents_OnBuildProjectBegin(string project, string projectconfig, string platform, string solutionconfig)
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            var eventTime = DateTime.Now;

            ProjectItem currentProject;
            if (BuildScope == BuildScopes.BuildScopeBatch)
            {
                currentProject = _locatorService.FindProjectItemInProjectsByUniqueName(_viewModel, project, projectconfig, platform);
                if (currentProject == null)
                    currentProject = _locatorService.AddProjectToVisibleProjectsByUniqueName(_viewModel, project, projectconfig, platform);
            }
            else
            {
                currentProject = _locatorService.FindProjectItemInProjectsByUniqueName(_viewModel, project);
                if (currentProject == null)
                    currentProject = _locatorService.AddProjectToVisibleProjectsByUniqueName(_viewModel, project);
            }

            lock (_buildingProjectsLockObject)
            {
                BuildingProjects.Add(currentProject);
            }

            var projectState = GetProjectState();
            OnBuildProjectBegin(new BuildProjectEventArgs(currentProject, projectState, eventTime, null));
        }

        private void BuildEvents_BuildInProcess(object state)
        {
            lock (_buildProcessLockObject)
            {
                if (BuildAction == BuildActions.BuildActionDeploy)
                    return;

                var token = (CancellationToken) state;
                while (!token.IsCancellationRequested)
                {
                    OnBuildProcess();

                    for (int i = 0; i < BuildInProcessQuantumSleep * BuildInProcessCountOfQuantumSleep; i += BuildInProcessQuantumSleep)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        System.Threading.Thread.Sleep(BuildInProcessQuantumSleep);
                    }
                }
            }
        }

        private void BuildEvents_OnBuildProjectDone(string project, string projectconfig, string platform, string solutionconfig, bool success)
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            var eventTime = DateTime.Now;
            var currentProject = _locatorService.GetCurrentProject(_viewModel, BuildScope, project, projectconfig, platform);

            lock (_buildingProjectsLockObject)
            {
                BuildingProjects.Remove(currentProject);
            }

            BuildedProject buildedProject = BuildedProjects[currentProject];
            buildedProject.Success = success;

            ProjectState projectState;
            switch (BuildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    if (success)
                    {
                        if (_viewModel.ControlSettings.GeneralSettings.ShowWarningSignForBuilds && buildedProject.ErrorsBox.WarningsCount > 0)
                            projectState = ProjectState.BuildWarning;
                        else
                        {
                            bool upToDate = (_buildLogger != null && _buildLogger.Projects != null
                                         && !_buildLogger.Projects.Exists(t => t.FileName == buildedProject.FileName));
                            if (upToDate)
                            {
                                // Because ErrorBox will be empty if project is UpToDate.
                                buildedProject.ErrorsBox = currentProject.ErrorsBox;
                            }
                            projectState = upToDate ? ProjectState.UpToDate : ProjectState.BuildDone;
                        }
                    }
                    else
                    {
                        bool canceled = (_buildCancelled && buildedProject.ErrorsBox.ErrorsCount == 0);
                        projectState = canceled ? ProjectState.BuildCancelled : ProjectState.BuildError;
                    }
                    break;

                case BuildActions.BuildActionClean:
                    projectState = success ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(BuildAction));
            }

            buildedProject.ProjectState = projectState;
            OnBuildProjectDone(new BuildProjectEventArgs(currentProject, projectState, eventTime, buildedProject));
        }

        private void BuildEvents_OnBuildDone()
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            if (CurrentBuildState != BuildState.InProgress)
            {
                // Start command (F5), when Build is not required.
                return;
            }

            _buildProcessCancellationToken.Cancel();

            BuildedSolution = BuildingSolution;
            BuildingSolution = null;

            lock (_buildingProjectsLockObject)
            {
                BuildingProjects.Clear();
            }

            BuildFinishTime = DateTime.Now;
            CurrentBuildState = BuildState.Done;

            OnBuildDone();
        }

        private void OnBuildProjectBegin(BuildProjectEventArgs e)
        {
            try
            {
                ProjectItem currentProject = e.ProjectItem;
                currentProject.State = e.ProjectState;
                currentProject.BuildFinishTime = null;
                currentProject.BuildStartTime = e.EventTime;

                _viewModel.OnBuildProjectBegin();
                if (BuildScope == BuildScopes.BuildScopeSolution &&
                    (BuildAction == BuildActions.BuildActionBuild ||
                     BuildAction == BuildActions.BuildActionRebuildAll))
                {
                    currentProject.BuildOrder = _viewModel.BuildProgressViewModel.CurrentQueuePosOfBuildingProject;
                }

                if (!_viewModel.ProjectsList.Contains(currentProject))
                    _viewModel.ProjectsList.Add(currentProject);
                else if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                    _viewModel.OnPropertyChanged(nameof(BuildVisionPaneViewModel.GroupedProjectsList));
                _viewModel.CurrentProject = currentProject;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void OnBuildProjectDone(BuildProjectEventArgs e)
        {
            if (e.ProjectState == ProjectState.BuildError && _viewModel.ControlSettings.GeneralSettings.StopBuildAfterFirstError)
                CancelBuildAsync();

            try
            {
                ProjectItem currentProject = e.ProjectItem;
                currentProject.State = e.ProjectState;
                currentProject.BuildFinishTime = DateTime.Now;
                currentProject.UpdatePostBuildProperties(e.BuildedProjectInfo);

                if (!_viewModel.ProjectsList.Contains(currentProject))
                    _viewModel.ProjectsList.Add(currentProject);

                if (ReferenceEquals(_viewModel.CurrentProject, e.ProjectItem) && BuildingProjects.Any())
                    _viewModel.CurrentProject = BuildingProjects.Last();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }

            _viewModel.UpdateIndicators(this);

            try
            {
                _viewModel.OnBuildProjectDone(e.BuildedProjectInfo);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void OnBuildBegin()
        {
            try
            {
                _buildErrorIsNavigated = false;

                ApplyToolWindowStateAction(_viewModel.ControlSettings.WindowSettings.WindowActionOnBuildBegin);

                ViewModelHelper.UpdateSolution(_dte.Solution, _viewModel.SolutionItem);

                string message = BuildMessages.GetBuildBeginMajorMessage(_viewModel.SolutionItem, this, _viewModel.ControlSettings.BuildMessagesSettings);

                _statusBarNotificationService.ShowTextWithFreeze(message);
                _viewModel.TextCurrentState = message;
                _origTextCurrentState = message;
                _viewModel.ImageCurrentState = BuildImages.GetBuildBeginImage(this);
                _viewModel.ImageCurrentStateResult = null;

                ViewModelHelper.UpdateProjects(_viewModel.SolutionItem, Services.Dte.Solution);
                _viewModel.ProjectsList.Clear();

                if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                    _viewModel.ProjectsList.AddRange(_viewModel.SolutionItem.AllProjects);

                _viewModel.ResetIndicators(ResetIndicatorMode.ResetValue);

                int projectsCount = -1;
                switch (BuildScope)
                {
                    case BuildScopes.BuildScopeSolution:
                        if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                        {
                            projectsCount = _viewModel.ProjectsList.Count;
                        }
                        else
                        {
                            try
                            {
                                Solution solution = _dte.Solution;
                                if (solution != null)
                                    projectsCount = solution.GetProjects().Count;
                            }
                            catch (Exception ex)
                            {
                                ex.Trace("Unable to count projects in solution.");
                            }
                        }
                        break;

                    case BuildScopes.BuildScopeBatch:
                    case BuildScopes.BuildScopeProject:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(BuildScope));
                }

                _viewModel.OnBuildBegin(projectsCount, this);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void OnBuildProcess()
        {
            try
            {
                var labelsSettings = _viewModel.ControlSettings.BuildMessagesSettings;
                string msg = _origTextCurrentState + BuildMessages.GetBuildBeginExtraMessage(this, labelsSettings);

                _viewModel.TextCurrentState = msg;
                _statusBarNotificationService.ShowTextWithFreeze(msg);

                var buildingProjects = BuildingProjects;
                for (int i = 0; i < buildingProjects.Count; i++)
                    buildingProjects[i].RaiseBuildElapsedTimeChanged();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void OnBuildDone()
        {
            try
            {
                var settings = _viewModel.ControlSettings;

                if (BuildScope == BuildScopes.BuildScopeSolution)
                {
                    foreach (var projectItem in _viewModel.ProjectsList)
                    {
                        if (projectItem.State == ProjectState.Pending)
                            projectItem.State = ProjectState.Skipped;
                    }
                }

                _viewModel.UpdateIndicators(this);

                var message = BuildMessages.GetBuildDoneMessage(_viewModel.SolutionItem, this, settings.BuildMessagesSettings);
                var buildDoneImage = BuildImages.GetBuildDoneImage(this, _viewModel.ProjectsList, out ControlTemplate stateImage);

                _statusBarNotificationService.ShowText(message);
                _viewModel.TextCurrentState = message;
                _viewModel.ImageCurrentState = buildDoneImage;
                _viewModel.ImageCurrentStateResult = stateImage;
                _viewModel.CurrentProject = null;
                _viewModel.OnBuildDone(this);

                int errorProjectsCount = _viewModel.ProjectsList.Count(item => item.State.IsErrorState());
                if (errorProjectsCount > 0 || BuildIsCancelled)
                    ApplyToolWindowStateAction(settings.WindowSettings.WindowActionOnBuildError);
                else
                    ApplyToolWindowStateAction(settings.WindowSettings.WindowActionOnBuildSuccess);

                bool navigateToBuildFailureReason = (!BuildedProjects.BuildWithoutErrors
                                                     && settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnBuildDone);
                if (navigateToBuildFailureReason && BuildedProjects.Any(p => p.ErrorsBox.Errors.Any(NavigateToErrorItem)))
                {
                    _buildErrorIsNavigated = true;
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void OnBuildCancelled()
        {
            _viewModel.OnBuildCancelled(this);
        }

        private async void OnErrorRaised(object sender, BuildErrorRaisedEventArgs args)
        {
            bool buildNeedToCancel = (args.ErrorLevel == ErrorLevel.Error && _viewModel.ControlSettings.GeneralSettings.StopBuildAfterFirstError);
            if (buildNeedToCancel)
                await CancelBuildAsync();

            bool navigateToBuildFailureReason = (!_buildErrorIsNavigated
                                                 && args.ErrorLevel == ErrorLevel.Error
                                                 && _viewModel.ControlSettings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnErrorRaised);
            if (navigateToBuildFailureReason && args.ProjectInfo.ErrorsBox.Errors.Any(NavigateToErrorItem))
            {
                _buildErrorIsNavigated = true;
            }
        }

        private bool NavigateToErrorItem(ErrorItem errorItem)
        {
            if (string.IsNullOrEmpty(errorItem?.File) || string.IsNullOrEmpty(errorItem?.ProjectFile))
                return false;

            try
            {
                var projectItem = _viewModel.ProjectsList.FirstOrDefault(x => x.FullName == errorItem.ProjectFile);
                if (projectItem == null)
                    return false;


                var project = _dte.Solution.GetProject(x => x.UniqueName == projectItem.UniqueName);
                if (project == null)
                    return false;

                return project.NavigateToErrorItem(errorItem);
            }
            catch (Exception ex)
            {
                ex.Trace("Navigate to error item exception");
                return true;
            }
        }

        private void ApplyToolWindowStateAction(WindowStateAction windowStateAction)
        {
            switch (windowStateAction.State)
            {
                case WindowState.Nothing:
                    break;
                case WindowState.Show:
                    _toolWindowManager.Show();
                    break;
                case WindowState.ShowNoActivate:
                    _toolWindowManager.ShowNoActivate();
                    break;
                case WindowState.Hide:
                    _toolWindowManager.Hide();
                    break;
                case WindowState.Close:
                    _toolWindowManager.Close();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowStateAction));
            }
        }
    }
}
