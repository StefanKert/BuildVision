using System;
using System.ComponentModel.Composition;
using System.IO;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
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
        public BuildOutputLogger _buildLogger;

        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        private readonly IServiceProvider _serviceProvider;
        private Solution _solution;
        private BuildInformationModel _buildInformationModel;
        private BuildEvents _buildEvents;
        private DTE2 _dte;

        [ImportingConstructor]
        public BuildInformationProvider([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _buildInformationModel = new BuildInformationModel();
            _buildEvents = (serviceProvider.GetService(typeof(DTE)) as DTE).Events.BuildEvents;
            _buildEvents.OnBuildBegin += _buildEvents_OnBuildBegin;
        }

        private void _buildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            _buildInformationModel.BuildScope = (BuildScopes) scope; // we need to set buildscope explictly because it is not possible to get this via the other api
        }

        public IBuildInformationModel GetBuildInformationModel()
        {
            return _buildInformationModel;
        }

        public void BuildStarted(uint dwAction)
        {
            RegisterLogger();

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

        private void RegisterLogger()
        {
            _buildLogger = null;
            RegisterLoggerResult result = BuildOutputLogger.Register(_parsingErrorsLoggerId, Microsoft.Build.Framework.LoggerVerbosity.Quiet, out _buildLogger);
            if (result == RegisterLoggerResult.AlreadyExists)
            {
                _buildLogger.Projects?.Clear();
            }
        }
    }
}
