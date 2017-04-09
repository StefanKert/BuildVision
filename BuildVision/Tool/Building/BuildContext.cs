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
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;
using System.Linq;
using BuildVision.Contracts;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class BuildContext : IBuildInfo, IBuildDistributor
    {
        private readonly IPackageContext _packageContext;
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

        private bool _buildCancelled;
       
        private DateTime? _buildStartTime;
        private DateTime? _buildFinishTime;
        private readonly BuildedProjectsCollection _buildedProjects;
        private BuildedSolution _buildedSolution;
        private ControlViewModel _viewModel;

        public BuildActions? BuildAction { get; private set; }

        public BuildScopes? BuildScope { get; private set; }

        public BuildState CurrentBuildState { get; private set; }

        public bool BuildIsCancelled
        {
            get { return _buildCancelled && !_buildCancelledInternally; }
        }

        public DateTime? BuildStartTime
        {
            get { return _buildStartTime; }
        }

        public DateTime? BuildFinishTime
        {
            get { return _buildFinishTime; }
        }

        public BuildedProjectsCollection BuildedProjects
        {
            get { return _buildedProjects; }
        }

        public IReadOnlyList<ProjectItem> BuildingProjects
        {
            get { return _buildingProjects; }
        }

        public BuildedSolution BuildedSolution 
        {
            get { return _buildedSolution; }
        }

        public Project BuildScopeProject
        {
            get { return _buildScopeProject; }
        }

        public void OverrideBuildProperties(BuildActions? buildAction = null, BuildScopes? buildScope = null)
        {
            BuildAction = buildAction ?? BuildAction;
            BuildScope = buildScope ?? BuildScope;
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
            if (CurrentBuildState != BuildState.InProgress || _buildCancelled || _buildCancelledInternally)
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

        public BuildContext(IPackageContext packageContext, ControlViewModel viewModel)
        {
            _viewModel = viewModel;
            _buildedProjects = new BuildedProjectsCollection();
            _buildingProjects = new List<ProjectItem>();
            _buildingProjectsLockObject = ((ICollection)_buildingProjects).SyncRoot;

            _packageContext = packageContext;

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

        public ProjectItem FindProjectItem(object property, FindProjectProperty findProjectProperty)
        {
            ProjectItem found;
            List<ProjectItem> projList = _viewModel.ProjectsList.ToList();
            switch (findProjectProperty)
            {
                case FindProjectProperty.UniqueName:
                    var uniqueName = (string)property;
                    found = projList.FirstOrDefault(item => item.UniqueName == uniqueName);
                    break;

                case FindProjectProperty.FullName:
                    var fullName = (string)property;
                    found = projList.FirstOrDefault(item => item.FullName == fullName);
                    break;

                case FindProjectProperty.UniqueNameProjectDefinition:
                    {
                        var projDef = (UniqueNameProjectDefinition)property;
                        found = projList.FirstOrDefault(item => item.UniqueName == projDef.UniqueName
                                                      && item.Configuration == projDef.Configuration
                                                      && PlatformsIsEquals(item.Platform, projDef.Platform));
                    }
                    break;

                case FindProjectProperty.FullNameProjectDefinition:
                    {
                        var projDef = (FullNameProjectDefinition)property;
                        found = projList.FirstOrDefault(item => item.FullName == projDef.FullName
                                                      && item.Configuration == projDef.Configuration
                                                      && PlatformsIsEquals(item.Platform, projDef.Platform));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("findProjectProperty");
            }
            if (found != null)
                return found;

            Project proj;
            switch (findProjectProperty)
            {
                case FindProjectProperty.UniqueName:
                    var uniqueName = (string)property;
                    proj = _viewModel.SolutionItem.StorageSolution.GetProject(item => item.UniqueName == uniqueName);
                    break;

                case FindProjectProperty.FullName:
                    var fullName = (string)property;
                    proj = _viewModel.SolutionItem.StorageSolution.GetProject(item => item.FullName == fullName);
                    break;

                case FindProjectProperty.UniqueNameProjectDefinition:
                    {
                        var projDef = (UniqueNameProjectDefinition)property;
                        proj = _viewModel.SolutionItem.StorageSolution.GetProject(item => item.UniqueName == projDef.UniqueName
                                                                && item.ConfigurationManager.ActiveConfiguration.ConfigurationName == projDef.Configuration
                                                                && PlatformsIsEquals(item.ConfigurationManager.ActiveConfiguration.PlatformName, projDef.Platform));
                    }
                    break;

                case FindProjectProperty.FullNameProjectDefinition:
                    {
                        var projDef = (FullNameProjectDefinition)property;
                        proj = _viewModel.SolutionItem.StorageSolution.GetProject(item => item.FullName == projDef.FullName
                                                                && item.ConfigurationManager.ActiveConfiguration.ConfigurationName == projDef.Configuration
                                                                && PlatformsIsEquals(item.ConfigurationManager.ActiveConfiguration.PlatformName, projDef.Platform));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("findProjectProperty");
            }

            if (proj == null)
                return null;

            var newProjItem = new ProjectItem();
            ViewModelHelper.UpdateProperties(proj, newProjItem);
            _viewModel.ProjectsList.Add(newProjItem);
            return newProjItem;
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
                buildedProject.ErrorsBox.Keep(errorLevel, e, _packageContext.GetDTE().Solution.GetProject(x => x.UniqueName == projectItem.UniqueName));
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
            if (ProjectExtensions.IsProjectHidden(projectFile))
                return false;

            IDictionary<string, string> projectProperties = projectEntry.Properties;
            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                // TODO: Use find by FullNameProjectDefinition for the Batch Build only.
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                var projectDefinition = new FullNameProjectDefinition(projectFile, projectConfiguration, projectPlatform);
                projectItem = FindProjectItem(projectDefinition, FindProjectProperty.FullNameProjectDefinition);
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
                projectItem = FindProjectItem(projectFile, FindProjectProperty.FullName);
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

        private void BuildEvents_OnBuildProjectBegin(
            string project,
            string projectconfig,
            string platform,
            string solutionconfig)
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            var eventTime = DateTime.Now;

            ProjectItem currentProject;
            if (BuildScope == BuildScopes.BuildScopeBatch)
            {
                var projectDefinition = new UniqueNameProjectDefinition(project, projectconfig, platform);
                currentProject = FindProjectItem(projectDefinition, FindProjectProperty.UniqueNameProjectDefinition);
                if (currentProject == null)
                {
                    var proj = FindProjectItem(project, FindProjectProperty.UniqueName);
                    currentProject = (proj ?? new ProjectItem()).GetBatchBuildCopy(projectconfig, platform);
                }
            }
            else
            {
                currentProject = FindProjectItem(project, FindProjectProperty.UniqueName);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }

            lock (_buildingProjectsLockObject)
            {
                _buildingProjects.Add(currentProject);
            }

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
                    throw new ArgumentOutOfRangeException();
            }

            OnBuildProjectBegin(this, new BuildProjectEventArgs(currentProject, projectState, eventTime, null));
        }

        private void BuildEvents_OnBuildProjectDone(string project, string projectconfig,
            string platform, string solutionconfig, bool success)
        {
            if (BuildAction == BuildActions.BuildActionDeploy)
                return;

            var eventTime = DateTime.Now;

            ProjectItem currentProject;
            if (BuildScope == BuildScopes.BuildScopeBatch)
            {
                var projectDefinition = new UniqueNameProjectDefinition(project, projectconfig, platform);
                currentProject = FindProjectItem(projectDefinition, FindProjectProperty.UniqueNameProjectDefinition);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }
            else
            {
                currentProject = FindProjectItem(project, FindProjectProperty.UniqueName);
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
            switch (BuildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
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

                case BuildActions.BuildActionClean:
                    projectState = success ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                case BuildActions.BuildActionDeploy:
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

            CurrentBuildState = BuildState.InProgress;

            _buildStartTime = DateTime.Now;
            _buildFinishTime = null;
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

            _buildedSolution = _buildingSolution;
            _buildingSolution = null;

            lock (_buildingProjectsLockObject)
            {
                _buildingProjects.Clear();
            }

            _buildFinishTime = DateTime.Now;
            CurrentBuildState = BuildState.Done;

            OnBuildDone(this, EventArgs.Empty);
        }

        #endregion
    }
}