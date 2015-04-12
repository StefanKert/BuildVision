using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using AlekseyNagovitsyn.BuildVision.Core.Common;
using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Helpers;
using AlekseyNagovitsyn.BuildVision.Tool.Models;

using EnvDTE;

using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class BuildContext : BuildInfo, IBuildDistributor
    {
        private readonly IPackageContext _packageContext;
        private readonly FindProjectItemDelegate _findProjectItem;
        private readonly List<ProjectItem> _buildingProjects;
        private readonly object _buildingProjectsLockObject;

        /* ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
         * 
         * These variables should be declared within the class scope rather than inside the function.
         * Otherwise, the problem is that when the function scope is left by calling code, the local *Events variable is marked
         * as avilable for garbage collection. Eventually the variable is collected, the event is disconnected...
         * See https://msdn.microsoft.com/en-us/library/ms165650(v=vs.80).aspx, Eventing variables.
         */
        private readonly BuildEvents _buildEvents;
        private readonly WindowEvents _windowEvents;
        private readonly CommandEvents _commandEvents;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private readonly object _buildProcessLockObject = new object();

        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        private BuildOutputLogger _buildLogger;

        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private CancellationTokenSource _buildProcessCancellationToken;

        private BuildedSolution _buildingSolution;
        private Window _activeProjectContext;
        private Project _buildScopeProject;
        private bool _buildCancelledInternally;

        #region Implementation BuildInfo

        private vsBuildAction? _buildAction;
        private vsBuildScope? _buildScope;
        private bool _buildCancelled;
        private BuildState _currentState;
        private DateTime? _buildStartTime;
        private DateTime? _buildFinishTime;
        private readonly BuildedProjectsCollection _buildedProjects;
        private BuildedSolution _buildedSolution;

        public override vsBuildAction? BuildAction
        {
            get { return _buildAction; }
        }

        public override vsBuildScope? BuildScope
        {
            get { return _buildScope; }
        }

        public override bool BuildIsCancelled
        {
            get { return _buildCancelled && !_buildCancelledInternally; }
        }

        public override BuildState CurrentBuildState
        {
            get { return _currentState; }
        }

        public override DateTime? BuildStartTime
        {
            get { return _buildStartTime; }
        }

        public override DateTime? BuildFinishTime
        {
            get { return _buildFinishTime; }
        }

        public override BuildedProjectsCollection BuildedProjects
        {
            get { return _buildedProjects; }
        }

        public override IReadOnlyList<ProjectItem> BuildingProjects
        {
            get { return _buildingProjects; }
        }

        public override BuildedSolution BuildedSolution 
        {
            get { return _buildedSolution; }
        }

        public override void OverrideBuildProperties(vsBuildAction? buildAction = null, vsBuildScope? buildScope = null)
        {
            _buildAction = buildAction ?? _buildAction;
            _buildScope = buildScope ?? _buildScope;
        }

        public override Project BuildScopeProject
        {
            get { return _buildScopeProject; }
        }

        #endregion

        #region Implementation IBuildDistributor

        public event EventHandler OnBuildBegin = delegate { };
        public event EventHandler OnBuildProcess = delegate { };
        public event EventHandler OnBuildDone = delegate { };
        public event EventHandler OnBuildCancelled = delegate { };
        public event EventHandler<BuildProjectEventArgs> OnBuildProjectBegin = delegate { };
        public event EventHandler<BuildProjectEventArgs> OnBuildProjectDone = delegate { };
        public event EventHandler<BuildErrorRaisedEventArgs> OnErrorRaised = delegate { };

        public void CancelBuild()
        {
            if (_currentState != BuildState.InProgress || _buildCancelled || _buildCancelledInternally)
                return;

            try
            {
                _packageContext.GetDTE().ExecuteCommand("Build.Cancel");
                _buildCancelledInternally = true;
            }
            catch (Exception ex)
            {
                ex.Trace("Cancel build failed.");
            }
        }

        #endregion

        public BuildContext(IPackageContext packageContext, FindProjectItemDelegate findProjectItem)
        {
            _buildedProjects = new BuildedProjectsCollection();
            _buildingProjects = new List<ProjectItem>();
            _buildingProjectsLockObject = ((ICollection)_buildingProjects).SyncRoot;

            _packageContext = packageContext;
            _findProjectItem = findProjectItem;

            Events dteEvents = packageContext.GetDTE().Events;
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

        #region Parsing errors with logger

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
                if (_buildLogger.Projects != null)
                    _buildLogger.Projects.Clear();
            }
        }

        private bool VerifyLoggerBuildEvent(BuildOutputLogger loggerSender, 
            BuildEventArgs eventArgs, ErrorLevel errorLevel)
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

        private void EventSource_ErrorRaised(
            BuildOutputLogger loggerSender,
            LazyFormattedBuildEventArgs e,
            ErrorLevel errorLevel)
        {
            try
            {
                bool verified = VerifyLoggerBuildEvent(loggerSender, e, errorLevel);
                if (!verified)
                    return;

                int projectInstanceId = e.BuildEventContext.ProjectInstanceId;
                int projectContextId = e.BuildEventContext.ProjectContextId;

                BuildProjectContextEntry projectEntry = loggerSender.Projects.Find(t =>
                    t.InstanceId == projectInstanceId
                    && t.ContextId == projectContextId);

                if (projectEntry == null)
                {
                    TraceManager.Trace(
                        string.Format(
                            "Project entry not found by ProjectInstanceId='{0}' and ProjectContextId='{1}'.",
                            projectInstanceId,
                            projectContextId), 
                        EventLogEntryType.Warning);

                    return;
                }

                if (projectEntry.IsInvalid)
                    return;

                ProjectItem projectItem;
                if (!GetProjectItem(projectEntry, out projectItem))
                {
                    projectEntry.IsInvalid = true;
                    return;
                }

                BuildedProject buildedProject = _buildedProjects[projectItem];
                buildedProject.ErrorsBox.Keep(errorLevel, e);
                OnErrorRaised(this, new BuildErrorRaisedEventArgs(errorLevel, buildedProject));
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private bool GetProjectItem(BuildProjectContextEntry projectEntry, out ProjectItem projectItem)
        {
            projectItem = projectEntry.ProjectItem;
            if (projectItem != null)
                return true;

            string projectFile = projectEntry.FileName;
            if (SolutionProjectsExtensions.ProjectIsHidden(projectFile))
                return false;

            IDictionary<string, string> projectProperties = projectEntry.Properties;
            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                // TODO: Use find by FullNameProjectDefinition for the Batch Build only.
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                var projectDefinition = new FullNameProjectDefinition(projectFile, projectConfiguration, projectPlatform);
                projectItem = _findProjectItem(projectDefinition, FindProjectProperty.FullNameProjectDefinition);
                if (projectItem == null)
                {
                    TraceManager.Trace(
                        string.Format("Project Item not found by: FullName='{0}', Configuration='{1}, Platform='{2}'.", 
                            projectDefinition.FullName, 
                            projectDefinition.Configuration, 
                            projectDefinition.Platform),
                        EventLogEntryType.Warning);
                    return false;
                }
            }
            else
            {
                projectItem = _findProjectItem(projectFile, FindProjectProperty.FullName);
                if (projectItem == null)
                {
                    TraceManager.Trace(
                        string.Format("Project Item not found by FullName='{0}'.", projectFile),
                        EventLogEntryType.Warning);
                    return false;
                }
            }

            projectEntry.ProjectItem = projectItem;
            return true;
        }

        #endregion

        #region Common DTE event handlers

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
                    if (gotFocus.Project != null && !gotFocus.Project.ProjectIsHidden())
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

        private void BuildEvents_OnBuildProjectBegin(
            string project,
            string projectconfig,
            string platform,
            string solutionconfig)
        {
            if (_buildAction == vsBuildAction.vsBuildActionDeploy)
                return;

            var eventTime = DateTime.Now;

            ProjectItem currentProject;
            if (_buildScope == vsBuildScope.vsBuildScopeBatch)
            {
                var projectDefinition = new UniqueNameProjectDefinition(project, projectconfig, platform);
                currentProject = _findProjectItem(projectDefinition, FindProjectProperty.UniqueNameProjectDefinition);
                if (currentProject == null)
                {
                    var proj = _findProjectItem(project, FindProjectProperty.UniqueName);
                    currentProject = (proj ?? new ProjectItem()).GetBatchBuildCopy(projectconfig, platform);
                }
            }
            else
            {
                currentProject = _findProjectItem(project, FindProjectProperty.UniqueName);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }

            lock (_buildingProjectsLockObject)
            {
                _buildingProjects.Add(currentProject);
            }

            ProjectState projectState;
            switch (_buildAction)
            {
                case vsBuildAction.vsBuildActionBuild:
                case vsBuildAction.vsBuildActionRebuildAll:
                    projectState = ProjectState.Building;
                    break;

                case vsBuildAction.vsBuildActionClean:
                    projectState = ProjectState.Cleaning;
                    break;

                case vsBuildAction.vsBuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnBuildProjectBegin(this, new BuildProjectEventArgs(currentProject, projectState, eventTime, null));
        }

        private void BuildEvents_OnBuildProjectDone(string project, string projectconfig,
            string platform, string solutionconfig, bool success)
        {
            if (_buildAction == vsBuildAction.vsBuildActionDeploy)
                return;

            var eventTime = DateTime.Now;

            ProjectItem currentProject;
            if (_buildScope == vsBuildScope.vsBuildScopeBatch)
            {
                var projectDefinition = new UniqueNameProjectDefinition(project, projectconfig, platform);
                currentProject = _findProjectItem(projectDefinition, FindProjectProperty.UniqueNameProjectDefinition);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }
            else
            {
                currentProject = _findProjectItem(project, FindProjectProperty.UniqueName);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }

            lock (_buildingProjectsLockObject)
            {
                _buildingProjects.Remove(currentProject);
            }

            BuildedProject buildedProject = _buildedProjects[currentProject];
            buildedProject.Success = success;

            ProjectState projectState;
            switch (_buildAction)
            {
                case vsBuildAction.vsBuildActionBuild:
                case vsBuildAction.vsBuildActionRebuildAll:
                    if (success)
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
                    else
                    {
                        bool canceled = (_buildCancelled && buildedProject.ErrorsBox.ErrorsCount == 0);
                        projectState = canceled ? ProjectState.BuildCancelled : ProjectState.BuildError;
                    }
                    break;

                case vsBuildAction.vsBuildActionClean:
                    projectState = success ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                case vsBuildAction.vsBuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnBuildProjectDone(this, new BuildProjectEventArgs(currentProject, projectState, eventTime, buildedProject));
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (action == vsBuildAction.vsBuildActionDeploy)
                return;

            RegisterLogger();

            _currentState = BuildState.InProgress;

            _buildStartTime = DateTime.Now;
            _buildFinishTime = null;
            _buildAction = action;

            switch (scope)
            {
                case vsBuildScope.vsBuildScopeSolution:
                case vsBuildScope.vsBuildScopeBatch:
                case vsBuildScope.vsBuildScopeProject:
                    _buildScope = scope;
                    break;

                case 0:
                    // Scope may be 0 in case of Clean solution, then Start (F5).
                    _buildScope = vsBuildScope.vsBuildScopeSolution;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("scope");
            }

            if (scope == vsBuildScope.vsBuildScopeProject)
            {
                var projContext = _activeProjectContext;
                switch (projContext.Type)
                {
                    case vsWindowType.vsWindowTypeSolutionExplorer:
                        ////var solutionExplorer = (UIHierarchy)dte.Windows.Item(Constants.vsext_wk_SProjectWindow).Object;
                        var solutionExplorer = (UIHierarchy)projContext.Object;
                        var items = (Array)solutionExplorer.SelectedItems;
                        switch (items.Length)
                        {
                            case 0:
                                TraceManager.TraceError("Unable to get target projects in Solution Explorer (vsBuildScope.vsBuildScopeProject)");
                                _buildScopeProject = null;
                                break;

                            case 1:
                                var item = (UIHierarchyItem)items.GetValue(0);
                                _buildScopeProject = (Project)item.Object;
                                break;

                            default:
                                _buildScopeProject = null;
                                break;
                        }
                        break;

                    case vsWindowType.vsWindowTypeDocument:
                        _buildScopeProject = projContext.Project;
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported type of active project context for vsBuildScope.vsBuildScopeProject.");
                }
            }

            _buildCancelled = false;
            _buildCancelledInternally = false;

            _buildedProjects.Clear();
            lock (_buildingProjectsLockObject)
            {
                _buildingProjects.Clear();
            }

            _buildedSolution = null;
            _buildingSolution = new BuildedSolution(_packageContext.GetDTE().Solution);

            OnBuildBegin(this, EventArgs.Empty);

            _buildProcessCancellationToken = new CancellationTokenSource();
            Task.Factory.StartNew(BuildEvents_BuildInProcess,
                                    _buildProcessCancellationToken.Token,
                                    _buildProcessCancellationToken.Token);
        }

        private void BuildEvents_BuildInProcess(object state)
        {
            lock (_buildProcessLockObject)
            {
                if (_buildAction == vsBuildAction.vsBuildActionDeploy)
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
            if (_buildAction == vsBuildAction.vsBuildActionDeploy)
                return;

            if (_currentState != BuildState.InProgress)
            {
                // Start command (F5), when Build is not required.
                return;
            }

            _buildProcessCancellationToken.Cancel();

            _buildedSolution = _buildingSolution;
            _buildingSolution = null;

            lock (_buildingProjectsLockObject)
            {
                _buildingProjects.Clear();
            }

            _buildFinishTime = DateTime.Now;
            _currentState = BuildState.Done;

            OnBuildDone(this, EventArgs.Empty);
        }

        #endregion
    }
}