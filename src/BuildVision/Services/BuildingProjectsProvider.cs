using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.Services;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell;

namespace BuildVision.Core
{
    [Export(typeof(IBuildInformationProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BuildingProjectsProvider : IBuildingProjectsProvider
    {
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildInformationProvider _buildInformationProvider;
        private readonly IBuildOutputLogger _buildOutputLogger;
        private readonly IBuildProgressViewModel _buildProgressViewModel;
        private readonly IPackageSettingsProvider _packageSettingsProvider;
        private readonly IBuildService _buildService;
        private readonly IErrorNavigationService _errorNavigationService;
        private ObservableCollection<IProjectItem> _projects;

        [ImportingConstructor]
        public BuildingProjectsProvider(
            [Import(typeof(ISolutionProvider))] ISolutionProvider solutionProvider, 
            [Import(typeof(IBuildInformationProvider))] IBuildInformationProvider buildInformationProvider,
            [Import(typeof(IBuildOutputLogger))] IBuildOutputLogger buildOutputLogger,
            [Import(typeof(IBuildProgressViewModel))] IBuildProgressViewModel buildProgressViewModel,
            [Import(typeof(IPackageSettingsProvider))] IPackageSettingsProvider packageSettingsProvider,
            [Import(typeof(IBuildService))] IBuildService buildService,
            [Import(typeof(IErrorNavigationService))] IErrorNavigationService errorNavigationService)
        {
            _projects = new ObservableCollection<IProjectItem>();
            _solutionProvider = solutionProvider;
            _buildInformationProvider = buildInformationProvider;
            _buildOutputLogger = buildOutputLogger;
            _buildProgressViewModel = buildProgressViewModel;
            _packageSettingsProvider = packageSettingsProvider;
            _buildService = buildService;
            _errorNavigationService = errorNavigationService;
            _buildOutputLogger.OnErrorRaised += BuildOutputLogger_OnErrorRaised;
        }

        private void BuildOutputLogger_OnErrorRaised(BuildProjectContextEntry projectEntry, object e, ErrorLevel errorLevel)
        {
            try
            {
                if (!TryGetProjectItem(projectEntry, out var projectItem))
                {
                    projectEntry.IsInvalid = true;
                    return;
                }

                var errorItem = new ErrorItem(errorLevel);
                switch (errorLevel)
                {
                    case ErrorLevel.Message:
                        errorItem.Init((BuildMessageEventArgs) e);
                        break;

                    case ErrorLevel.Warning:
                        errorItem.Init((BuildWarningEventArgs) e);
                        break;
                    case ErrorLevel.Error:
                        errorItem.Init((BuildErrorEventArgs) e);
                        break;
                    default:  
                        break;
                }

                errorItem.VerifyValues();
                AddErrorItem(projectItem, errorItem);

                var args = new BuildErrorRaisedEventArgs(errorLevel, projectItem);
                bool buildNeedCancel = (args.ErrorLevel == ErrorLevel.Error && _packageSettingsProvider.Settings.GeneralSettings.StopBuildAfterFirstError);
                if (buildNeedCancel)
                {
                    _buildService.CancelBuildSolution();
                }

                bool navigateToBuildFailure = (args.ErrorLevel == ErrorLevel.Error && _packageSettingsProvider.Settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnErrorRaised);
                if (_packageSettingsProvider.Settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnBuildDone && !ErrorNavigationService.BuildErrorNavigated)
                {
                    _errorNavigationService.NavigateToErrorItem(errorItem);
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public void AddErrorItem(IProjectItem projectItem, ErrorItem errorItem)
        {
            switch (errorItem.Level)
            {
                case ErrorLevel.Message:
                    projectItem.MessagesCount++;
                    break;
                case ErrorLevel.Warning:
                    projectItem.WarningsCount++;
                    break;
                case ErrorLevel.Error:
                    projectItem.ErrorsCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }
            if (errorItem.Level != ErrorLevel.Error)
                return;

            int errorNumber = projectItem.Errors.Count + projectItem.Warnings.Count + projectItem.Messages.Count + 1;
            errorItem.Number = errorNumber;
            switch (errorItem.Level)
            {
                case ErrorLevel.Message:
                    projectItem.Messages.Add(errorItem);
                    break;

                case ErrorLevel.Warning:
                    projectItem.Warnings.Add(errorItem);
                    break;

                case ErrorLevel.Error:
                    projectItem.Errors.Add(errorItem);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("errorLevel");
            }
        }

        public void ReloadCurrentProjects()
        {
            _projects.Clear();
            _projects.AddRange(_solutionProvider.GetProjects());
        }

        public void ResetCurrentProjects()
        {
            _projects.Clear();
        }

        public ObservableCollection<IProjectItem> GetBuildingProjects()
        {
            return _projects;
        }

        public void ProjectBuildStarted(IProjectItem projectItem, uint dwAction)
        {
            var buildInformationModel = _buildInformationProvider.GetBuildInformationModel();
            if (buildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            try
            {
                var projInCollection = _projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForProjectItem(projectItem));
                if(projInCollection == null)
                {
                    _projects.Add(projectItem);
                    projInCollection = projectItem;
                }
                projInCollection.State = GetProjectState(buildInformationModel.BuildAction);
                projInCollection.BuildFinishTime = null;
                projInCollection.BuildStartTime = DateTime.Now;

                _buildProgressViewModel.OnBuildProjectBegin();

                if (buildInformationModel.BuildScope == BuildScopes.BuildScopeSolution &&
                    (buildInformationModel.BuildAction == BuildActions.BuildActionBuild ||
                     buildInformationModel.BuildAction == BuildActions.BuildActionRebuildAll))
                {
                    projInCollection.BuildOrder = _buildProgressViewModel.CurrentQueuePosOfBuildingProject;
                }
                buildInformationModel.CurrentProject = projInCollection;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public bool TryGetProjectItem(BuildProjectContextEntry projectEntry, out IProjectItem projectItem)
        {
            projectItem = projectEntry.ProjectItem;
            if (projectItem != null)
                return true;

            string projectFile = projectEntry.FileName;
            if (ProjectExtensions.IsProjectHidden(projectFile))
                return false;

            var projectProperties = projectEntry.Properties;
            var project = _projects.FirstOrDefault(x => x.FullName == projectFile);

            if (projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = _projects.First(item => $"{item.FullName}-{item.Configuration}|{item.Platform.Replace(" ","")}" == $"{projectFile}-{projectConfiguration}|{projectPlatform}");
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
                return false;
            }

            projectEntry.ProjectItem = projectItem;
            return true;
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

        public void ProjectBuildFinished(string projectIdentifier, bool success, bool canceled)
        {
            var buildInformationModel = _buildInformationProvider.GetBuildInformationModel();
            if (buildInformationModel.BuildAction == BuildActions.BuildActionDeploy)
            {
                return;
            }

            var currentProject = _projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == projectIdentifier);
            var buildAction = _buildInformationProvider.GetBuildInformationModel().BuildAction;
            ProjectState projectState;
            switch (buildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    if (success)
                    {
                        if (_packageSettingsProvider.Settings.GeneralSettings.ShowWarningSignForBuilds && currentProject.WarningsCount > 0)
                        {
                            projectState = ProjectState.BuildWarning;
                        }
                        else
                        {
                            projectState = _buildOutputLogger.IsProjectUpToDate(currentProject) ? ProjectState.UpToDate : ProjectState.BuildDone;
                            if(projectState == ProjectState.UpToDate)
                            {
                                // do i have to set errorbox here?
                            }
                        }
                    }
                    else
                    {
                        bool buildCancelled = (canceled && currentProject.ErrorsCount == 0);
                        projectState = buildCancelled ? ProjectState.BuildCancelled : ProjectState.BuildError;
                    }
                    break;

                case BuildActions.BuildActionClean:
                    projectState = success ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
            currentProject.Success = success;
            currentProject.State = projectState;
            currentProject.BuildFinishTime = DateTime.Now;

            if (currentProject.State == ProjectState.BuildError && _packageSettingsProvider.Settings.GeneralSettings.StopBuildAfterFirstError)
            {
                _buildService.CancelBuildSolution();
            }
             
            buildInformationModel.SucceededProjectsCount = _projects.Count(x => x.State == ProjectState.BuildDone || x.State == ProjectState.CleanDone);
            buildInformationModel.FailedProjectsCount = _projects.Count(x => x.State == ProjectState.BuildError || x.State == ProjectState.CleanError);
            buildInformationModel.WarnedProjectsCount = _projects.Count(x => x.State == ProjectState.BuildWarning);
            buildInformationModel.UpToDateProjectsCount = _projects.Count(x => x.State == ProjectState.UpToDate);
            buildInformationModel.MessagesCount = _projects.Sum(x => x.MessagesCount);
            buildInformationModel.ErrorCount = _projects.Sum(x => x.ErrorsCount);
            buildInformationModel.WarningsCount = _projects.Sum(x => x.WarningsCount);

            if (buildInformationModel.CurrentProject == null)
            {
                buildInformationModel.CurrentProject = GetBuildingProjects().Last();
            }

            _buildProgressViewModel.OnBuildProjectDone(success);
        }
    }
}
