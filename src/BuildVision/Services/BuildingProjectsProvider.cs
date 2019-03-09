using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Models;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    [Export(typeof(IBuildInformationProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BuildingProjectsProvider : IBuildingProjectsProvider
    {
        public BuildOutputLogger _buildLogger;

        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");
        private readonly IServiceProvider _serviceProvider;
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildInformationProvider _buildInformationProvider;
        private ObservableCollection<ProjectItem> _projects;


        [ImportingConstructor]
        public BuildingProjectsProvider(
            [Import(typeof(ISolutionProvider))] ISolutionProvider solutionProvider, 
            [Import(typeof(IBuildInformationProvider))] IBuildInformationProvider buildInformationProvider)
        {
            _projects = new ObservableCollection<ProjectItem>();
            _solutionProvider = solutionProvider;
            _buildInformationProvider = buildInformationProvider;
        }

        public void ReloadCurrentProjects()
        {
            _projects.Clear();
            _projects.AddRange(_solutionProvider.GetProjects());
        }

        public ObservableCollection<ProjectItem> GetBuildingProjects()
        {
            return _projects;
        }

        public void ProjectBuildStarted(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction)
        {
            try
            {
                var projectItem = _projects.FirstOrDefault(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForInteropTypes(pHierProj, pCfgProj));
                if (projectItem == null)
                {
                    // In this case we are executing a batch build so we need to add the projectitem manually
                    projectItem = new UI.Models.ProjectItem();
                    var configPair = pCfgProj.ToConfigurationTuple();
                    SolutionProjectsExtensions.UpdateProperties(pHierProj.ToProject(), projectItem, configPair.Item1, configPair.Item2);
                    _projects.Add(projectItem);
                }

                projectItem.State = GetProjectState(_buildInformationProvider.GetBuildInformationModel().BuildAction);
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

        public void ProjectBuildFinished(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, bool succeess, bool canceled)
        {
            var currentProject = _projects.First(item => ProjectIdentifierGenerator.GetIdentifierForProjectItem(item) == ProjectIdentifierGenerator.GetIdentifierForInteropTypes(pHierProj, pCfgProj));
            currentProject.State = ProjectState.BuildDone;
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
        }
    }
}
