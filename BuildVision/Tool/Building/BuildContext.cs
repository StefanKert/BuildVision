using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildVision.Contracts;
using BuildVision.Core;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.ViewModels;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using ErrorItem = BuildVision.Contracts.ErrorItem;
using ProjectItem = BuildVision.UI.Models.ProjectItem;

namespace BuildVision.Tool.Building
{
    public class BuildContext : IBuildInfo, IBuildDistributor
    { 
        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private const string CancelBuildCommand = "Build.Cancel";

        private readonly object _buildingProjectsLockObject;
        private readonly object _buildProcessLockObject = new object();

        private readonly IPackageContext _packageContext;
        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        private BuildOutputLogger _buildLogger;
        private CancellationTokenSource _buildProcessCancellationToken;
        private bool _buildCancelledInternally;
        private Window _activeProjectContext;
        private readonly ControlViewModel _viewModel;
        private bool _buildCancelled;
        private readonly BuildEvents _buildEvents;
        private readonly WindowEvents _windowEvents;
        private readonly CommandEvents _commandEvents;

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

        public BuildContext(IPackageContext packageContext, ControlViewModel viewModel)
        {
            _viewModel = viewModel;
            BuildedProjects = new BuildedProjectsCollection();
            BuildingProjects = new List<ProjectItem>();
            _buildingProjectsLockObject = ((ICollection)BuildingProjects).SyncRoot;

            _packageContext = packageContext;

            var dteEvents = packageContext.GetDTE().Events;
            _buildEvents = dteEvents.BuildEvents;
            _windowEvents = dteEvents.WindowEvents;
            _commandEvents = dteEvents.CommandEvents;

            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildDone += (s, e) => BuildEvents_OnBuildDone();
            _buildEvents.OnBuildProjConfigBegin += BuildEvents_OnBuildProjectBegin;
            _buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjectDone;

            _windowEvents.WindowActivated += WindowEvents_WindowActivated;

            _commandEvents.AfterExecute += CommandEvents_AfterExecute;
        }

        public void OverrideBuildProperties(BuildActions? buildAction = null, BuildScopes? buildScope = null)
        {
            BuildAction = buildAction ?? BuildAction;
            BuildScope = buildScope ?? BuildScope;
        }

        public void CancelBuild()
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
                var cancelBuildTask = Task.Run(() => _packageContext.GetDTE().ExecuteCommand(CancelBuildCommand));
                cancelBuildTask.Wait(10000); 
                _buildCancelledInternally = true;
            }
            catch (Exception ex)
            {
                ex.Trace("Cancel build failed.");
            }
        }

        public ProjectItem FindProjectItemInProjectsByUniqueName(string uniqueName, string configuration, string platform)
        {
            return _viewModel.ProjectsList.FirstOrDefault(item => item.UniqueName == uniqueName && item.Configuration == configuration && PlatformsIsEquals(item.Platform, platform));
        }

        public ProjectItem FindProjectItemInProjectsByUniqueName(string uniqueName)
        {
            return _viewModel.ProjectsList.FirstOrDefault(item => item.UniqueName == uniqueName);
        }

        public ProjectItem AddProjectToVisibleProjectsByUniqueName(string uniqueName)
        {
            var proj = _viewModel.SolutionItem.AllProjects.FirstOrDefault(x => x.UniqueName == uniqueName);
            if (proj == null)
                throw new InvalidOperationException();
            _viewModel.ProjectsList.Add(proj);
            return proj;
        }

        public ProjectItem AddProjectToVisibleProjectsByUniqueName(string uniqueName, string configuration, string platform)
        {
            ProjectItem currentProject = _viewModel.SolutionItem.AllProjects.FirstOrDefault(item => item.UniqueName == uniqueName
                                                            && item.Configuration == configuration
                                                          && PlatformsIsEquals(item.Platform, platform));
            if (currentProject == null)
            {
                currentProject = _viewModel.SolutionItem.AllProjects.FirstOrDefault(x => x.UniqueName == uniqueName);
                if (currentProject == null)
                    throw new InvalidOperationException();
                currentProject = currentProject.GetBatchBuildCopy(configuration, platform);
                _viewModel.SolutionItem.AllProjects.Add(currentProject);
            }

            _viewModel.ProjectsList.Add(currentProject);
            return currentProject;
        }

        private bool PlatformsIsEquals(string platformName1, string platformName2)
        {
            if (string.Compare(platformName1, platformName2, StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;

            // The ambiguity between Project.ActiveConfiguration.PlatformName and
            // ProjectStartedEventArgs.ProjectPlatform in Microsoft.Build.Utilities.Logger
            // (see BuildOutputLogger).
            bool isAnyCpu1 = (platformName1 == "Any CPU" || platformName1 == "AnyCPU");
            bool isAnyCpu2 = (platformName2 == "Any CPU" || platformName2 == "AnyCPU");
            if (isAnyCpu1 && isAnyCpu2)
                return true;

            return false;
        }

        private void RegisterLogger()
        {
            _buildLogger = null;

            // Same Verbosity as in the Error List.
            const LoggerVerbosity LoggerVerbosity = LoggerVerbosity.Quiet;
            RegisterLoggerResult result = BuildOutputLogger.Register(_parsingErrorsLoggerId, LoggerVerbosity, out _buildLogger);

            if (result == RegisterLoggerResult.RegisterSuccess)
            {
                var eventSource = _buildLogger.EventSource;
                eventSource.MessageRaised += (s, e) => EventSource_ErrorRaised(_buildLogger, e, ErrorLevel.Message);
                eventSource.WarningRaised += (s, e) => EventSource_ErrorRaised(_buildLogger, e, ErrorLevel.Warning);
                eventSource.ErrorRaised += (s, e) => EventSource_ErrorRaised(_buildLogger, e, ErrorLevel.Error);
            }
            else if (result == RegisterLoggerResult.AlreadyExists)
            {
                _buildLogger.Projects?.Clear();
            }
        }

        private bool VerifyLoggerBuildEvent(BuildOutputLogger loggerSender, BuildEventArgs eventArgs, ErrorLevel errorLevel)
        {
            var bec = eventArgs.BuildEventContext;
            bool becIsInvalid = (bec == null
                                 || bec == BuildEventContext.Invalid
                                 || bec.ProjectContextId == BuildEventContext.InvalidProjectContextId
                                 || bec.ProjectInstanceId == BuildEventContext.InvalidProjectInstanceId);
            if (becIsInvalid)
                return false;

            if (errorLevel == ErrorLevel.Message)
            {
                var messageEventArgs = (BuildMessageEventArgs)eventArgs;
                bool isUserMessage = (messageEventArgs.Importance == MessageImportance.High && loggerSender.IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                                    || (messageEventArgs.Importance == MessageImportance.Normal && loggerSender.IsVerbosityAtLeast(LoggerVerbosity.Normal))
                                    || (messageEventArgs.Importance == MessageImportance.Low && loggerSender.IsVerbosityAtLeast(LoggerVerbosity.Detailed));

                if (!isUserMessage)
                    return false;
            }

            return true;
        }

        private void EventSource_ErrorRaised(BuildOutputLogger loggerSender, LazyFormattedBuildEventArgs e, ErrorLevel errorLevel)
        {
            try
            {
                bool verified = VerifyLoggerBuildEvent(loggerSender, e, errorLevel);
                if (!verified)
                    return;

                int projectInstanceId = e.BuildEventContext.ProjectInstanceId;
                int projectContextId = e.BuildEventContext.ProjectContextId;

                var projectEntry = loggerSender.Projects.Find(t => t.InstanceId == projectInstanceId && t.ContextId == projectContextId);
                if (projectEntry == null)
                {
                    TraceManager.Trace(string.Format("Project entry not found by ProjectInstanceId='{0}' and ProjectContextId='{1}'.", projectInstanceId, projectContextId), EventLogEntryType.Warning);
                    return;
                }
                if (projectEntry.IsInvalid)
                    return;

                if (!GetProjectItem(projectEntry, out var projectItem))
                {
                    projectEntry.IsInvalid = true;
                    return;
                }

                BuildedProject buildedProject = BuildedProjects[projectItem];
                var errorItem = new ErrorItem(errorLevel, (eI) =>
                {
                    _packageContext.GetDTE().Solution.GetProject(x => x.UniqueName == projectItem.UniqueName).NavigateToErrorItem(eI);
                });

                switch (errorLevel)
                {
                    case ErrorLevel.Message:
                        Init(errorItem, (BuildMessageEventArgs)e);
                        break;

                    case ErrorLevel.Warning:
                        Init(errorItem, (BuildWarningEventArgs)e);
                        break;

                    case ErrorLevel.Error:
                        Init(errorItem, (BuildErrorEventArgs)e);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("errorLevel");
                }
                errorItem.VerifyValues();
                buildedProject.ErrorsBox.Add(errorItem);
                OnErrorRaised(this, new BuildErrorRaisedEventArgs(errorLevel, buildedProject));
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void Init(ErrorItem item, BuildErrorEventArgs e)
        {
            item.Code = e.Code;
            item.File = e.File;
            item.ProjectFile = e.ProjectFile;
            item.LineNumber = e.LineNumber;
            item.ColumnNumber = e.ColumnNumber;
            item.EndLineNumber = e.EndLineNumber;
            item.EndColumnNumber = e.EndColumnNumber;
            item.Subcategory = e.Subcategory;
            item.Message = e.Message;
        }

        private void Init(ErrorItem item, BuildWarningEventArgs e)
        {
            item.Code = e.Code;
            item.File = e.File;
            item.ProjectFile = e.ProjectFile;
            item.LineNumber = e.LineNumber;
            item.ColumnNumber = e.ColumnNumber;
            item.EndLineNumber = e.EndLineNumber;
            item.EndColumnNumber = e.EndColumnNumber;
            item.Subcategory = e.Subcategory;
            item.Message = e.Message;
        }

        private void Init(ErrorItem item, BuildMessageEventArgs e)
        {
            item.Code = e.Code;
            item.File = e.File;
            item.ProjectFile = e.ProjectFile;
            item.LineNumber = e.LineNumber;
            item.ColumnNumber = e.ColumnNumber;
            item.EndLineNumber = e.EndLineNumber;
            item.EndColumnNumber = e.EndColumnNumber;
            item.Subcategory = e.Subcategory;
            item.Message = e.Message;
        }

        private bool GetProjectItem(BuildProjectContextEntry projectEntry, out ProjectItem projectItem)
        {
            projectItem = projectEntry.ProjectItem;
            if (projectItem != null)
                return true;

            string projectFile = projectEntry.FileName;
            if (ProjectExtensions.IsProjectHidden(projectFile))
                return false;

            IDictionary<string, string> projectProperties = projectEntry.Properties;
            var project = _viewModel.ProjectsList.FirstOrDefault(x => x.FullName == projectFile);


            if (BuildScope == BuildScopes.BuildScopeBatch && projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = FindProjectItemInProjectsByUniqueName(project.UniqueName, projectConfiguration, projectPlatform);
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
                projectItem = FindProjectItemInProjectsByUniqueName(project.UniqueName);
                if (projectItem == null)
                {
                    TraceManager.Trace(string.Format("Project Item not found by FullName='{0}'.", projectFile), EventLogEntryType.Warning);
                    return false;
                }
            }

            projectEntry.ProjectItem = projectItem;
            return true;
        }

        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
        {
            if (gotFocus == null)
                return;

            switch (gotFocus.Type)
            {
                case vsWindowType.vsWindowTypeSolutionExplorer:
                    _activeProjectContext = gotFocus;
                    break;

                case vsWindowType.vsWindowTypeDocument:
                case vsWindowType.vsWindowTypeDesigner:
                case vsWindowType.vsWindowTypeCodeWindow:
                    if (gotFocus.Project != null && !gotFocus.Project.IsHidden())
                        _activeProjectContext = gotFocus;
                    break;

                default:
                    return;
            }
        }

        private void CommandEvents_AfterExecute(string guid, int id, object customIn, object customOut)
        {
            if (id == (int)VSConstants.VSStd97CmdID.CancelBuild
                && Guid.Parse(guid) == VSConstants.GUID_VSStandardCommandSet97)
            {
                _buildCancelled = true;
                if (!_buildCancelledInternally)
                    OnBuildCancelled(this, EventArgs.Empty);
            }
        }

        private void BuildEvents_OnBuildProjectBegin(string project, string projectconfig, string platform, string solutionconfig)
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            var eventTime = DateTime.Now;

            ProjectItem currentProject;
            if (BuildScope == BuildScopes.BuildScopeBatch)
            {
                currentProject = FindProjectItemInProjectsByUniqueName(project, projectconfig, platform);
                if (currentProject == null)
                    currentProject = AddProjectToVisibleProjectsByUniqueName(project, projectconfig, platform);
            }
            else
            {
                currentProject = FindProjectItemInProjectsByUniqueName(project);
                if (currentProject == null)
                    currentProject = AddProjectToVisibleProjectsByUniqueName(project);
            }

            lock (_buildingProjectsLockObject)
            {
                BuildingProjects.Add(currentProject);
            }

            var projectState = GetProjectState();
            OnBuildProjectBegin(this, new BuildProjectEventArgs(currentProject, projectState, eventTime, null));
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

        private void BuildEvents_OnBuildProjectDone(string project, string projectconfig, string platform, string solutionconfig, bool success)
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            var eventTime = DateTime.Now;
            var currentProject = GetCurrentProject(project, projectconfig, platform);

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
            OnBuildProjectDone(this, new BuildProjectEventArgs(currentProject, projectState, eventTime, buildedProject));
        }

        private ProjectItem GetCurrentProject(string project, string projectconfig, string platform)
        {
            ProjectItem currentProject;
            if (BuildScope == BuildScopes.BuildScopeBatch)
            {
                currentProject = FindProjectItemInProjectsByUniqueName(project, projectconfig, platform);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }
            else
            {
                currentProject = FindProjectItemInProjectsByUniqueName(project);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }

            return currentProject;
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
                    BuildScope = (BuildScopes)vsBuildScope.vsBuildScopeSolution;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("scope");
            }

            if (scope == vsBuildScope.vsBuildScopeProject)
            {
                var selectedProjects = _packageContext.GetDTE().ActiveSolutionProjects as object[];
                if (selectedProjects?.Length == 1)
                {
                    var projectItem = new ProjectItem();
                    ViewModelHelper.UpdateProperties((Project)selectedProjects[0], projectItem);
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
            var solution = _packageContext.GetDTE().Solution;
            BuildingSolution = new BuildedSolution(solution.FullName, solution.FileName);

            OnBuildBegin(this, EventArgs.Empty);

            _buildProcessCancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(BuildEvents_BuildInProcess, _buildProcessCancellationToken.Token, _buildProcessCancellationToken.Token);
        }

        private void BuildEvents_BuildInProcess(object state)
        {
            lock (_buildProcessLockObject)
            {
                if (BuildAction == BuildActions.BuildActionDeploy)
                    return;

                var token = (CancellationToken)state;
                while (!token.IsCancellationRequested)
                {
                    OnBuildProcess(this, EventArgs.Empty);

                    for (int i = 0; i < BuildInProcessQuantumSleep * BuildInProcessCountOfQuantumSleep; i += BuildInProcessQuantumSleep)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        System.Threading.Thread.Sleep(BuildInProcessQuantumSleep);
                    }
                }
            }
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

            OnBuildDone(this, EventArgs.Empty);
        }

        public event EventHandler OnBuildBegin = delegate { };
        public event EventHandler OnBuildProcess = delegate { };
        public event EventHandler OnBuildDone = delegate { };
        public event EventHandler OnBuildCancelled = delegate { };
        public event EventHandler<BuildProjectEventArgs> OnBuildProjectBegin = delegate { };
        public event EventHandler<BuildProjectEventArgs> OnBuildProjectDone = delegate { };
        public event EventHandler<BuildErrorRaisedEventArgs> OnErrorRaised = delegate { };
    }
}
