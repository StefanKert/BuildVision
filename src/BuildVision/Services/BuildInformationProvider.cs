using System;
using System.ComponentModel.Composition;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
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

        private Solution _solution;
        private BuildInformationModel _buildInformationModel;
        private BuildEvents _buildEvents;
        private DTE2 _dte;

        [ImportingConstructor]
        public BuildInformationProvider(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(IBuildOutputLogger))] IBuildOutputLogger buildOutputLogger)
        {
            _serviceProvider = serviceProvider;
            _buildOutputLogger = buildOutputLogger;
            _buildInformationModel = new BuildInformationModel();
            _buildEvents = (serviceProvider.GetService(typeof(DTE)) as DTE).Events.BuildEvents;
            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            _buildInformationModel.BuildScope = (BuildScopes) scope; // we need to set buildscope explictly because it is not possible to get this via the other api
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
            //TODO use settings 
            //string message = new BuildMessagesFactory(new UI.Settings.Models.BuildMessagesSettings()).GetBuildBeginMajorMessage(VisualStudioSolution);
            //_statusBarNotificationService.ShowTextWithFreeze(message);
            //_origTextCurrentState = message;
            //VisualStudioSolution.StateMessage = _origTextCurrentState; // Set message
        }

        private int GetProjectsCount(int projectsCount)
        {
            switch (_buildInformationModel.BuildScope)
            {
                case BuildScopes.BuildScopeSolution:
                    try
                    {
                        Solution solution = _dte.Solution;
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
            //_buildInformationModel.StateMessage = _origTextCurrentState + BuildMessages.GetBuildBeginExtraMessage(this, labelsSettings);
        }

        public void BuildFinished(bool success, bool modified, bool canceled)
        {
            //try
            //{
            //    var settings = _viewModel.ControlSettings;

            //    SetFinalStateForUnfinishedProjects();

            //    // Update header? This might be happening impliclty when updating solution
            //    //_viewModel.UpdateIndicators(this);

            //    var message = BuildMessages.GetBuildDoneMessage(_viewModel.SolutionItem, this, settings.BuildMessagesSettings);
            //    var buildDoneImage = BuildImages.GetBuildDoneImage(this, _viewModel.ProjectsList, out ControlTemplate stateImage);

            //    _statusBarNotificationService.ShowText(message);
            //    _viewModel.TextCurrentState = message;
            //    _viewModel.ImageCurrentState = buildDoneImage;
            //    _viewModel.ImageCurrentStateResult = stateImage;
            //    _viewModel.CurrentProject = null;
            //    _viewModel.OnBuildDone(this);

            //    ApplyWindowState(settings);
            //    NavigateToBuildErrorIfNeeded(settings);
            //}
            //catch (Exception ex)
            //{
            //    ex.TraceUnknownException();
            //}

            _buildInformationModel.BuildFinishTime = DateTime.Now;
            if(success)
                _buildInformationModel.CurrentBuildState = BuildState.Done;
            else if(canceled)
                _buildInformationModel.CurrentBuildState = BuildState.Cancelled;
            else 
                _buildInformationModel.CurrentBuildState = BuildState.Failed;
        }
    }
}
