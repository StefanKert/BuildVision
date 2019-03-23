using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Exports;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Models;
using Microsoft.Build.Framework;

namespace BuildVision.Core
{
    [Export(typeof(IBuildInformationProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BuildingProjectsProvider : IBuildingProjectsProvider
    {
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildInformationProvider _buildInformationProvider;
        private readonly IBuildOutputLogger _buildOutputLogger;
        private readonly IPackageContext _packageContext;
        private ObservableCollection<IProjectItem> _projects;

        [ImportingConstructor]
        public BuildingProjectsProvider(
            [Import(typeof(ISolutionProvider))] ISolutionProvider solutionProvider, 
            [Import(typeof(IBuildInformationProvider))] IBuildInformationProvider buildInformationProvider,
            [Import(typeof(IBuildOutputLogger))] IBuildOutputLogger buildOutputLogger)
        {
            _projects = new ObservableCollection<IProjectItem>();
            _solutionProvider = solutionProvider;
            _buildInformationProvider = buildInformationProvider;
            _buildOutputLogger = buildOutputLogger;
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
                        errorItem.VerifyValues();
                        break;
                }

                projectItem.ErrorsBox.Add(errorItem);
                //OnErrorRaised(this, new BuildErrorRaisedEventArgs(errorLevel, projectItem));
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public void ReloadCurrentProjects()
        {
            _projects.Clear();
            _projects.AddRange(_solutionProvider.GetProjects());
        }

        public ObservableCollection<IProjectItem> GetBuildingProjects()
        {
            return _projects;
        }

        public void ProjectBuildStarted(IProjectItem projectItem, uint dwAction)
        {
            try
            {
                var projInCollection = _projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForProjectItem(projectItem));
                if(projInCollection == null)
                {
                    _projects.Add(projectItem);
                    projInCollection = projectItem;
                }
                projInCollection.State = GetProjectState(_buildInformationProvider.GetBuildInformationModel().BuildAction);
                projInCollection.BuildFinishTime = null;
                projInCollection.BuildStartTime = DateTime.Now;

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

        public void ProjectBuildFinished(string projectIdentifier, bool succeess, bool canceled)
        {
            var currentProject = _projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == projectIdentifier);
            var buildAction = _buildInformationProvider.GetBuildInformationModel().BuildAction;
            ProjectState projectState;
            switch (buildAction)
            {
                case BuildActions.BuildActionBuild:
                case BuildActions.BuildActionRebuildAll:
                    if (succeess)
                    {
                        if (currentProject.ErrorsBox.WarningsCount > 0)
                            projectState = ProjectState.BuildWarning;
                        else
                        {
                            projectState = _buildOutputLogger.IsProjectUpToDate(currentProject) ? ProjectState.UpToDate : ProjectState.BuildDone;
                        }
                    }
                    else
                    {
                        //bool canceled = (_buildCancelled && currentProject.ErrorsBox.ErrorsCount == 0);
                        projectState = canceled ? ProjectState.BuildCancelled : ProjectState.BuildError;
                    }
                    break;

                case BuildActions.BuildActionClean:
                    projectState = succeess ? ProjectState.CleanDone : ProjectState.CleanError;
                    break;

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
            currentProject.State = projectState;

            var buildInformationModel = _buildInformationProvider.GetBuildInformationModel();
            buildInformationModel.SucceededProjectsCount = _projects.Count(x => x.State == ProjectState.BuildDone || x.State == ProjectState.CleanDone);
            buildInformationModel.FailedProjectsCount = _projects.Count(x => x.State == ProjectState.BuildError || x.State == ProjectState.CleanError);
            buildInformationModel.WarnedProjectsCount = _projects.Count(x => x.State == ProjectState.BuildWarning);
            buildInformationModel.UpToDateProjectsCount = _projects.Count(x => x.State == ProjectState.UpToDate);
            buildInformationModel.MessagesCount = _projects.Sum(x => x.MessagesCount);
            buildInformationModel.ErrorCount = _projects.Sum(x => x.ErrorsCount);
            buildInformationModel.WarningsCount = _projects.Sum(x => x.WarningsCount);

            //OnBuildProjectDone(new BuildProjectEventArgs(currentProject, projectState, eventTime, buildedProject));

            //if (currentProject.State == ProjectState.BuildError && _packageContext..ControlSettings.GeneralSettings.StopBuildAfterFirstError)
            //    CancelBuildAsync();

            try
            {
                currentProject.BuildFinishTime = DateTime.Now;
                //currentProject.UpdatePostBuildProperties(e.BuildedProjectInfo);

                //if (ReferenceEquals(_viewModel.CurrentProject, e.ProjectItem) && BuildingProjects.Any())
                //    _viewModel.CurrentProject = BuildingProjects.Last();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }

            //_viewModel.UpdateIndicators(this);

            //try
            //{
            //    _viewModel.OnBuildProjectDone(e.BuildedProjectInfo);
            //}
            //catch (Exception ex)
            //{
            //    ex.TraceUnknownException();
            //}
        }
    }
}
