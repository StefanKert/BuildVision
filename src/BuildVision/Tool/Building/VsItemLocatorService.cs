using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BuildVision.Contracts;
using BuildVision.Helpers;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using ProjectItem = BuildVision.UI.Models.ProjectItem;

namespace BuildVision.Tool.Building
{
    public class VsItemLocatorService : IVsItemLocatorService
    {
        public ProjectItem FindProjectItemInProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName, string configuration, string platform)
        {
            return viewModel.ProjectsList.FirstOrDefault(item => item.UniqueName == uniqueName && item.Configuration == configuration && PlatformsIsEquals(item.Platform, platform));
        }

        public ProjectItem FindProjectItemInProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName)
        {
            return viewModel.ProjectsList.FirstOrDefault(item => item.UniqueName == uniqueName);
        }

        public ProjectItem AddProjectToVisibleProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName)
        {
            var proj = viewModel.SolutionItem.AllProjects.FirstOrDefault(x => x.UniqueName == uniqueName);
            if (proj == null)
                throw new InvalidOperationException();
            viewModel.ProjectsList.Add(proj);
            return proj;
        }

        public ProjectItem AddProjectToVisibleProjectsByUniqueName(IBuildVisionPaneViewModel viewModel, string uniqueName, string configuration, string platform)
        {
            var currentProject = viewModel.SolutionItem.AllProjects.FirstOrDefault(item => item.UniqueName == uniqueName
                                                            && item.Configuration == configuration
                                                          && PlatformsIsEquals(item.Platform, platform));
            if (currentProject == null)
            {
                currentProject = viewModel.SolutionItem.AllProjects.FirstOrDefault(x => x.UniqueName == uniqueName);
                if (currentProject == null)
                    throw new InvalidOperationException();
                currentProject = currentProject.GetBatchBuildCopy(configuration, platform);
                viewModel.SolutionItem.AllProjects.Add(currentProject);
            }

            viewModel.ProjectsList.Add(currentProject);
            return currentProject;
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

        public ProjectItem GetCurrentProject(IBuildVisionPaneViewModel viewModel, BuildScopes? buildScope, string project, string projectconfig, string platform)
        {
            ProjectItem currentProject;
            if (buildScope == BuildScopes.BuildScopeBatch)
            {
                currentProject = FindProjectItemInProjectsByUniqueName(viewModel, project, projectconfig, platform);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }
            else
            {
                currentProject = FindProjectItemInProjectsByUniqueName(viewModel, project);
                if (currentProject == null)
                    throw new InvalidOperationException();
            }

            return currentProject;
        }

        public bool GetProjectItem(IBuildVisionPaneViewModel viewModel, BuildScopes? buildScope, BuildProjectContextEntry projectEntry, out ProjectItem projectItem)
        {
            projectItem = projectEntry.ProjectItem;
            if (projectItem != null)
                return true;

            string projectFile = projectEntry.FileName;
            if (ProjectExtensions.IsProjectHidden(projectFile))
                return false;

            var projectProperties = projectEntry.Properties;
            var project = viewModel.ProjectsList.FirstOrDefault(x => x.FullName == projectFile);


            if (buildScope == BuildScopes.BuildScopeBatch && projectProperties.ContainsKey("Configuration") && projectProperties.ContainsKey("Platform"))
            {
                string projectConfiguration = projectProperties["Configuration"];
                string projectPlatform = projectProperties["Platform"];
                projectItem = FindProjectItemInProjectsByUniqueName(viewModel, project.UniqueName, projectConfiguration, projectPlatform);
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
                projectItem = FindProjectItemInProjectsByUniqueName(viewModel, project.UniqueName);
                if (projectItem == null)
                {
                    TraceManager.Trace(string.Format("Project Item not found by FullName='{0}'.", projectFile), EventLogEntryType.Warning);
                    return false;
                }
            }

            projectEntry.ProjectItem = projectItem;
            return true;
        }
    }
}
