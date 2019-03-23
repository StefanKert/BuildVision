using System;
using System.ComponentModel.Composition;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
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
        private readonly IBuildOutputLogger _buildOutputLogger;
        private readonly IStatusBarNotificationService _statusBarNotificationService;
        private readonly IBuildMessagesFactory _buildMessagesFactory;
        private Solution _solution;
        private BuildInformationModel _buildInformationModel;
        private BuildEvents _buildEvents;
        private DTE2 _dte;
        private string _origTextCurrentState;

        [ImportingConstructor]
        public BuildInformationProvider(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(IBuildOutputLogger))] IBuildOutputLogger buildOutputLogger,
            [Import(typeof(IStatusBarNotificationService))] IStatusBarNotificationService statusBarNotificationService,
            [Import(typeof(IBuildMessagesFactory))] IBuildMessagesFactory buildMessagesFactory)
        {
            _serviceProvider = serviceProvider;
            _buildOutputLogger = buildOutputLogger;
            _statusBarNotificationService = statusBarNotificationService;
            _buildMessagesFactory = buildMessagesFactory;
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
        }

        public IBuildInformationModel GetBuildInformationModel()
        {
            return _buildInformationModel;
        }

        public void BuildStarted(uint dwAction)
        {
            _buildOutputLogger.Attach();

            _buildInformationModel.SucceededProjectsCount = 0;
            _buildInformationModel.FailedProjectsCount = 0;
            _buildInformationModel.WarnedProjectsCount = 0;
            _buildInformationModel.UpToDateProjectsCount = 0;
            _buildInformationModel.MessagesCount = 0;
            _buildInformationModel.WarnedProjectsCount = 0;
            _buildInformationModel.ErrorCount = 0;
            _buildInformationModel.StateMessage = "";
            _buildInformationModel.BuildStartTime = DateTime.Now;
            _buildInformationModel.BuildFinishTime = null;
            _buildInformationModel.CurrentBuildState = BuildState.InProgress;
            _buildInformationModel.BuildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS) dwAction);
        }

        private int GetProjectsCount(int projectsCount)
        {
            switch (_buildInformationModel.BuildScope)
            {
                case BuildScopes.BuildScopeSolution:
                    try
                    {
                        var solution = _dte.Solution;
                        if (solution != null)
                            projectsCount = _solution.GetProjects().Count;
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
            _buildInformationModel.StateMessage = _origTextCurrentState + _buildMessagesFactory.GetBuildBeginExtraMessage(_buildInformationModel);
        }

        public void BuildFinished(bool success, bool modified, bool canceled)
        {
            _buildInformationModel.BuildFinishTime = DateTime.Now;
            if (success)
                _buildInformationModel.CurrentBuildState = BuildState.Done;
            else if (canceled)
                _buildInformationModel.CurrentBuildState = BuildState.Cancelled;
            else
                _buildInformationModel.CurrentBuildState = BuildState.Failed;


            var message = _buildMessagesFactory.GetBuildDoneMessage(_buildInformationModel);
            _statusBarNotificationService.ShowText(message);
            _buildInformationModel.StateMessage = message;
            //_viewModel.OnBuildDone(this);

            ///ApplyWindowState(settings);
            //NavigateToBuildErrorIfNeeded(settings);
        }
    }
}
