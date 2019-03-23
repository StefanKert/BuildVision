using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.Services;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    [Export(typeof(IBuildInformationProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BuildInformationProvider : IBuildInformationProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IPackageSettingsProvider _packageSettingsProvider;
        private readonly IBuildProgressViewModel _buildProgressViewModel;
        private readonly IErrorNavigationService _errorNavigationService;
        private readonly IBuildOutputLogger _buildOutputLogger;
        private readonly IStatusBarNotificationService _statusBarNotificationService;
        private readonly IBuildMessagesFactory _buildMessagesFactory;
        private readonly IWindowStateService _windowStateService;
        private Solution _solution;
        private BuildInformationModel _buildInformationModel;
        private BuildEvents _buildEvents;
        private DTE2 _dte;
        private string _origTextCurrentState;

        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private readonly object _buildProcessLockObject;
        private CancellationTokenSource _buildProcessCancellationToken;
  
        [ImportingConstructor]
        public BuildInformationProvider(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(IBuildOutputLogger))] IBuildOutputLogger buildOutputLogger,
            [Import(typeof(IStatusBarNotificationService))] IStatusBarNotificationService statusBarNotificationService,
            [Import(typeof(IBuildMessagesFactory))] IBuildMessagesFactory buildMessagesFactory,
            [Import(typeof(IWindowStateService))] IWindowStateService windowStateService,
            [Import(typeof(IPackageSettingsProvider))] IPackageSettingsProvider packageSettingsProvider,
            [Import(typeof(IBuildProgressViewModel))] IBuildProgressViewModel buildProgressViewModel,
            [Import(typeof(IErrorNavigationService))] IErrorNavigationService errorNavigationService)
        {
            _serviceProvider = serviceProvider;
            _packageSettingsProvider = packageSettingsProvider;
            _buildProgressViewModel = buildProgressViewModel;
            _errorNavigationService = errorNavigationService;
            _buildOutputLogger = buildOutputLogger;
            _statusBarNotificationService = statusBarNotificationService;
            _buildMessagesFactory = buildMessagesFactory;
            _windowStateService = windowStateService;
            _buildInformationModel = new BuildInformationModel();
            _buildEvents = (serviceProvider.GetService(typeof(DTE)) as DTE).Events.BuildEvents;
            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            _buildInformationModel.BuildScope = (BuildScopes) scope; // we need to set buildscope explictly because it is not possible to get this via the other api
                                                                     //TODO use settings 
            string message = _buildMessagesFactory.GetBuildBeginMajorMessage(_buildInformationModel);
            _statusBarNotificationService.ShowTextWithFreeze(message);
            _origTextCurrentState = message;
            _buildInformationModel.StateMessage = _origTextCurrentState;

            var projectsCount = GetProjectsCount();
            _buildProgressViewModel.OnBuildBegin(_buildInformationModel, projectsCount);
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

        public IBuildInformationModel GetBuildInformationModel()
        {
            return _buildInformationModel;
        }

        public void BuildStarted(uint dwAction)
        {
            ErrorNavigationService.BuildErrorNavigated = false;
            _buildOutputLogger.Attach();

            ResetBuildInformationModel();

            _buildInformationModel.BuildStartTime = DateTime.Now;
            _buildInformationModel.BuildFinishTime = null;
            _buildInformationModel.CurrentBuildState = BuildState.InProgress;
            _buildInformationModel.BuildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS) dwAction);

            _buildProcessCancellationToken = new CancellationTokenSource();
            _windowStateService.ApplyToolWindowStateAction(_packageSettingsProvider.Settings.WindowSettings.WindowActionOnBuildBegin);
            System.Threading.Tasks.Task.Run(() => Run(_buildProcessCancellationToken.Token), _buildProcessCancellationToken.Token);
        }

        private int GetProjectsCount()
        {
            int projectsCount = -1;
            switch (_buildInformationModel.BuildScope)
            {
                case BuildScopes.BuildScopeSolution:
                    try
                    {
                        var solution = _serviceProvider.GetSolution();
                        if (solution != null)
                            projectsCount = solution.GetProjects().Count;
                    }
                    catch (Exception ex)
                    {
                        ex.Trace("Unable to count projects in solution.");
                    }

                    break;

                case BuildScopes.BuildScopeBatch:
                case BuildScopes.BuildScopeProject:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_buildInformationModel.BuildScope));
            }

            return projectsCount;
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

        public void BuildFinished(IEnumerable<IProjectItem> projects, bool success, bool canceled)
        {
            if (_buildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            if (_buildInformationModel.BuildScope == BuildScopes.BuildScopeSolution)
            {
                foreach (var projectItem in projects)
                {
                    if (projectItem.State == ProjectState.Pending)
                        projectItem.State = ProjectState.Skipped;
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
            _buildProgressViewModel.OnBuildDone();

            if(_buildInformationModel.FailedProjectsCount > 0 || canceled)
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
                    foreach (var project in projects)
                    {
                        if (ErrorNavigationService.BuildErrorNavigated)
                            break;
                        foreach (var error in project.ErrorsBox)
                        {
                            if (ErrorNavigationService.BuildErrorNavigated)
                                break;
                            _errorNavigationService.NavigateToErrorItem(error);
                        }
                    }
                }
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
    }
}
