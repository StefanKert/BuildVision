using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Helpers;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Helpers;
using BuildVision.UI.ViewModels;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{

    public class SolutionBuildEvents : IVsUpdateSolutionEvents2, IVsUpdateSolutionEvents4
    {
        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        public BuildOutputLogger _buildLogger; // This needs to be static because it is shared accross multiple instances

        public VisualStudioSolution VisualStudioSolution { get; }

        private BuildEvents _buildEvents;
        private bool _buildCancelled;
        private bool _buildCancelledInternally;
        private bool _buildErrorIsNavigated;
        private string _origTextCurrentState;

        public SolutionBuildEvents(VisualStudioSolution solutionBuildState)
        {
            VisualStudioSolution = solutionBuildState;
        }

        public void UpdateSolution_BeginUpdateAction(uint dwAction)
        {
            _buildCancelled = false;
            _buildCancelledInternally = false;
            _buildErrorIsNavigated = false;

            RegisterLogger();

            VisualStudioSolution.Projects.Clear();
            VisualStudioSolution.Projects.AddRange(Services.Dte2.Solution.GetProjectItems());
            VisualStudioSolution.BuildStartTime = DateTime.Now;
            VisualStudioSolution.BuildFinishTime = null;
            VisualStudioSolution.CurrentBuildState = BuildState.InProgress;
            VisualStudioSolution.BuildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS) dwAction);

            //TODO use settings 
            string message = new BuildMessagesFactory(new UI.Settings.Models.BuildMessagesSettings()).GetBuildBeginMajorMessage(VisualStudioSolution);
            //_statusBarNotificationService.ShowTextWithFreeze(message);
            _origTextCurrentState = message;
            //VisualStudioSolution.StateMessage = _origTextCurrentState; // Set message
            try
            {
                //ApplyToolWindowStateAction(_viewModel.ControlSettings.WindowSettings.WindowActionOnBuildBegin);
                //if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                //     _viewModel.ProjectsList.AddRange(_viewModel.SolutionItem.AllProjects);

                // Reset HeaderViewModel
                //_viewModel.ResetIndicators(ResetIndicatorMode.ResetValue);
                //int projectsCount = -1;
                //projectsCount = GetProjectsCount(projectsCount);
                //_viewModel.OnBuildBegin(projectsCount, this);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }

            //_buildProcessCancellationToken = new CancellationTokenSource();
            //// Startbackground process 
            ////Task.Factory.StartNew(BuildEvents_BuildInProcess, _buildProcessCancellationToken.Token, _buildProcessCancellationToken.Token);
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            try
            {
                var projectItem = VisualStudioSolution.Projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForInteropTypes(pHierProj, pCfgProj));
                if (projectItem == null)
                {
                    // In this case we are executing a batch build so we need to add the projectitem manually
                    projectItem = new UI.Models.ProjectItem();
                    var configPair = pCfgProj.ToConfigurationTuple();
                    SolutionProjectsExtensions.UpdateProperties(pHierProj.ToProject(), projectItem, configPair.Item1, configPair.Item2);
                    VisualStudioSolution.Projects.Add(projectItem);
                }

                projectItem.State = GetProjectState(VisualStudioSolution.BuildAction);
                projectItem.BuildFinishTime = null;
                projectItem.BuildStartTime = DateTime.Now;

                //  _viewModel.OnBuildProjectBegin();
                //if (BuildScope == BuildScopes.BuildScopeSolution &&
                //    (BuildAction == BuildActions.BuildActionBuild ||
                //     BuildAction == BuildActions.BuildActionRebuildAll))
                //{
                //    currentProject.BuildOrder = _viewModel.BuildProgressViewModel.CurrentQueuePosOfBuildingProject;
                //}
                //if (!_viewModel.ProjectsList.Contains(currentProject))
                //    _viewModel.ProjectsList.Add(currentProject);
                //else if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                //    _viewModel.OnPropertyChanged(nameof(BuildVisionPaneViewModel.GroupedProjectsList));
                //_viewModel.CurrentProject = currentProject;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            var currentProject = VisualStudioSolution.Projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForInteropTypes(pHierProj, pCfgProj));
            Debug.WriteLine($"UpdateProjectCfg_Done {currentProject.UniqueName} ({currentProject.Configuration}|{currentProject.Platform})");
            //currentProject.Success = fSuccess == 1;
            //ProjectState projectState;
            //switch (SolutionBuildState.BuildAction)
            //{
            //    case BuildActions.BuildActionBuild:
            //    case BuildActions.BuildActionRebuildAll:
            //        if (currentProject.Success)
            //        {
            //            if (_viewModel.ControlSettings.GeneralSettings.ShowWarningSignForBuilds && buildedProject.ErrorsBox.WarningsCount > 0)
            //                projectState = ProjectState.BuildWarning;
            //            else
            //            {
            //                bool upToDate = (_buildLogger != null && _buildLogger.Projects != null
            //                             && !_buildLogger.Projects.Exists(t => t.FileName == buildedProject.FileName));
            //                if (upToDate)
            //                {
            //                    // Because ErrorBox will be empty if project is UpToDate.
            //                    buildedProject.ErrorsBox = currentProject.ErrorsBox;
            //                }
            //                projectState = upToDate ? ProjectState.UpToDate : ProjectState.BuildDone;
            //            }
            //        }
            //        else
            //        {
            //            bool canceled = (_buildCancelled && buildedProject.ErrorsBox.ErrorsCount == 0);
            //            projectState = canceled ? ProjectState.BuildCancelled : ProjectState.BuildError;
            //        }
            //        break;

            //    case BuildActions.BuildActionClean:
            //        projectState = fSuccess == 1 ? ProjectState.CleanDone : ProjectState.CleanError;
            //        break;

            //    case BuildActions.BuildActionDeploy:
            //        throw new InvalidOperationException("vsBuildActionDeploy not supported");

            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(SolutionBuildState.BuildAction));
            //}

            //buildedProject.ProjectState = projectState;
            //OnBuildProjectDone(new BuildProjectEventArgs(currentProject, projectState, eventTime, buildedProject));

            //Debug.WriteLine($"UpdateProjectCfg_Done {proj.UniqueName} ({projConfiguration}) ({slnConfiguration}");

            //if (e.ProjectState == ProjectState.BuildError && _viewModel.ControlSettings.GeneralSettings.StopBuildAfterFirstError)
            //    CancelBuildAsync();

            //try
            //{
            //    ProjectItem currentProject = e.ProjectItem;
            //    currentProject.State = e.ProjectState;
            //    currentProject.BuildFinishTime = DateTime.Now;
            //    currentProject.UpdatePostBuildProperties(e.BuildedProjectInfo);

            //    if (!_viewModel.ProjectsList.Contains(currentProject))
            //        _viewModel.ProjectsList.Add(currentProject);

            //    if (ReferenceEquals(_viewModel.CurrentProject, e.ProjectItem) && BuildingProjects.Any())
            //        _viewModel.CurrentProject = BuildingProjects.Last();
            //}
            //catch (Exception ex)
            //{
            //    ex.TraceUnknownException();
            //}

            //_viewModel.UpdateIndicators(this);

            //try
            //{
            //    _viewModel.OnBuildProjectDone(e.BuildedProjectInfo);
            //}
            //catch (Exception ex)
            //{
            //    ex.TraceUnknownException();
            //}
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            if (VisualStudioSolution.CurrentBuildState == BuildState.InProgress)
            {
                // Start command (F5), when Build is not required.
                return VSConstants.S_OK;
            }

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

            VisualStudioSolution.BuildFinishTime = DateTime.Now;
            VisualStudioSolution.CurrentBuildState = BuildState.Done;
            return VSConstants.S_OK;
        }

        private ProjectState GetProjectState(BuildActions buildAction)
        {
            switch (buildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    return ProjectState.Building;

                case BuildActions.BuildActionClean:
                    return ProjectState.Cleaning;

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
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
            RegisterLoggerResult result = BuildOutputLogger.Register(_parsingErrorsLoggerId, Microsoft.Build.Framework.LoggerVerbosity.Quiet, out _buildLogger);
            if (result == RegisterLoggerResult.AlreadyExists)
            {
                _buildLogger.Projects?.Clear();
            }
        }

        //public bool GetProjectItem(IBuildVisionPaneViewModel viewModel, BuildProjectContextEntry projectEntry, out ProjectItem projectItem)
        //{
        //    projectItem = projectEntry.ProjectItem;
        //    if (projectItem != null)
        //        return true;

        //    string projectFile = projectEntry.FileName;
        //    if (ProjectExtensions.IsProjectHidden(projectFile))
        //        return false;

        //    var projectProperties = projectEntry.Properties;
        //    var project = viewModel.ProjectsList.FirstOrDefault(x => x.FullName == projectFile);


        //    if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
        //    {
        //        string projectConfiguration = projectProperties["Configuration"];
        //        string projectPlatform = projectProperties["Platform"];
        //        projectItem = FindProjectItemInProjectsByUniqueName(viewModel, project.UniqueName, projectConfiguration, projectPlatform);
        //        if (projectItem == null)
        //        {
        //            TraceManager.Trace(
        //                string.Format("Project Item not found by: UniqueName='{0}', Configuration='{1}, Platform='{2}'.",
        //                    project.UniqueName,
        //                    projectConfiguration,
        //                    projectPlatform),
        //                EventLogEntryType.Warning);
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    projectEntry.ProjectItem = projectItem;
        //    return true;
        //}

        #region Interface Implementation

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public void UpdateSolution_EndUpdateAction(uint dwAction)
        {
        }

        public void OnActiveProjectCfgChangeBatchBegin()
        {
        }

        public void OnActiveProjectCfgChangeBatchEnd()
        {
        }

        public void UpdateSolution_QueryDelayFirstUpdateAction(out int pfDelay)
        {
            pfDelay = 0;
        }

        public void UpdateSolution_BeginFirstUpdateAction()
        {
        }

        public void UpdateSolution_EndLastUpdateAction()
        {
        }
        #endregion
    }
}
