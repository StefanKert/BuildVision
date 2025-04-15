﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildVision.Common.Logging;
using BuildVision.Core;
using BuildVision.UI;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using ProjectItem = BuildVision.UI.Models.ProjectItem;

namespace BuildVision.Helpers
{
    public static class SolutionProjectsExtensions
    {
        private static IList<Project> _cachedProjects;

        public static SolutionModel ToSolutionBuildState(this Solution solution)
        {
            var solutionItem = new SolutionModel();
            try
            {
                if (solution == null)
                {
                    solutionItem.Name = Resources.GridCellNATextInBrackets;
                    solutionItem.FullName = Resources.GridCellNATextInBrackets;
                    solutionItem.IsEmpty = true;
                }
                else if (string.IsNullOrEmpty(solution.FileName))
                {
                    if (solution.Count != 0 /* projects count */)
                    {
                        var project = solution.Item(1);
                        solutionItem.Name = Path.GetFileNameWithoutExtension(project.FileName);
                        solutionItem.FullName = project.FullName;
                        solutionItem.IsEmpty = false;
                    }
                    else
                    {
                        solutionItem.Name = Resources.GridCellNATextInBrackets;
                        solutionItem.FullName = Resources.GridCellNATextInBrackets;
                        solutionItem.IsEmpty = true;
                    }
                }
                else
                {
                    solutionItem.Name = Path.GetFileNameWithoutExtension(solution.FileName);
                    solutionItem.FullName = solution.FullName;
                    solutionItem.IsEmpty = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.ForContext<Solution>().Error(ex, "Failed to load solution {FullName}", solution?.FullName);

                solutionItem.Name = Resources.GridCellNATextInBrackets;
                solutionItem.FullName = Resources.GridCellNATextInBrackets;
                solutionItem.IsEmpty = true;
            }
            return solutionItem;
        }

        public static Project FirstOrDefaultProject(this Solution solution, Func<Project, bool> cond) => _cachedProjects.FirstOrDefault(cond);

        public static Project FirstProject(this Solution solution, Func<Project, bool> cond) => _cachedProjects.First(cond);

        public static IList<ProjectItem> GetProjectItems(this IVsSolution solution)
        {
            IList<Project> dteProjects = new List<Project>();
            try
            {
                dteProjects = solution.GetProjects();
                _cachedProjects = dteProjects;
            }
            catch (Exception ex)
            {
                solution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out object fullName);
                LogManager.ForContext<IVsSolution>().Error(ex, "Failed to get projectitems for solution {FullName}", fullName);
            }

            var projectItems = new List<ProjectItem>(dteProjects.Count);
            foreach (var project in dteProjects)
            {
                try
                {
                    //var msBuildProject = project.GetMsBuildProject();
                    var projectItem = new ProjectItem();
                    UpdateProperties(project, projectItem);
                    projectItems.Add(projectItem);
                }
                catch (Exception ex)
                {
                    solution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out object fullName);
                    LogManager.ForContext<IVsSolution>().Error(ex, "Failed to update project properties for solution {FullName}", fullName);
                }
            }

            return projectItems;
        }

        public static IList<Project> GetProjects(this IVsSolution solution)
        {
            var list = new List<Project>();
            foreach (var proj in solution.GetAllProjectHierarchies())
            {
                var s = proj.ToProject();
                if (!(s is Project project))
                {
                    continue;
                }
                try
                {
                    if (project.FileName.EndsWith(".shproj")) // Shared Projects shouldn´t be displayed in BuildVision
                    {
                        continue;
                    }

                    if (!project.IsHidden())
                    {
                        list.Add(project);
                    }
                }
                catch (Exception ex)
                {
                    solution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out object fullName);
                    LogManager.ForContext<IVsSolution>().Error(ex, "Failed to load project with name {UniqueName} for solution {FullName}", project.UniqueName, fullName);
                }
            }
            return list;
        }

        public static void UpdateProperties(Project project, ProjectItem projectItem, string configuration = null, string platform = null)
        {
            if (project != null)
            {
                object projObject;
                try
                {
                    projObject = project.Object;
                }
                catch (Exception ex)
                {
                    LogManager.ForContext<Solution>().Error(ex, "Failed to load project.Object for project {UniqueName}", project?.UniqueName);
                    projObject = null;
                }

                try
                {
                    if (projObject == null)
                    {
                        projectItem.UniqueName = project.UniqueName;
                        projectItem.Name = project.Name;
                        return;
                    }

                    UpdateNameProperties(project, projectItem);
                    projectItem.Language = project.GetLanguageName();
                    
                    projectItem.CommonType = ProjectExtensions.GetProjectType(project.Kind, Community.VisualStudio.Toolkit.VS.Shell.GetVsVersionAsync().Result.ToString()/* "12.0" */);
                }
                catch (Exception ex)
                {
                    LogManager.ForContext<Solution>().Error(ex, "Failed to updatename properties project properties for project {UniqueName}", project?.UniqueName);
                }

                #region Set ActiveConfiguration (Configuration and Platform)

                if (configuration != null && platform != null)
                {
                    projectItem.Configuration = configuration;
                    projectItem.Platform = platform;
                }
                else
                {
                    Configuration config;
                    try
                    {
                        config = project.ConfigurationManager?.ActiveConfiguration;
                    }
                    catch (Exception ex)
                    {
                        LogManager.ForContext<Solution>().Error(ex, "Failed to get active configuration for {UniqueName}", project?.UniqueName);
                        config = null;
                    }

                    if (config != null)
                    {
                        projectItem.Configuration = config.ConfigurationName;
                        projectItem.Platform = config.PlatformName;
                    }
                    else
                    {
                        if (project.ConfigurationManager == null)
                        {
                            LogManager.ForContext<Solution>().Warning("ConfigurationManager for project {UniqueName} was null.", project?.UniqueName);
                        }
                        else if (project.ConfigurationManager.ActiveConfiguration == null)
                        {
                            LogManager.ForContext<Solution>().Warning("ActiveConfiguration for project {UniqueName} was null.", project?.UniqueName);
                        }
                        projectItem.Configuration = @"N\A";
                        projectItem.Platform = @"N\A";
                    }
                }

                #endregion

                try
                {
                    projectItem.Framework = project.GetFrameworkString();
                    var flavourTypes = project.GetFlavourTypes().ToList();
                    projectItem.FlavourType = string.Join("; ", flavourTypes);
                    projectItem.MainFlavourType = flavourTypes.FirstOrDefault();
                    projectItem.OutputType = project.GetOutputType();
                    projectItem.ExtenderNames = project.GetExtenderNames();
                    projectItem.RootNamespace = project.GetRootNamespace();
                }
                catch (Exception ex)
                {
                    LogManager.ForContext<Solution>().Error(ex, "Failed to load settings for project {UniqueName}", project?.UniqueName);
                }
            }
        }

        private static void UpdateNameProperties(Project project, ProjectItem projectItem)
        {
            try
            {
                projectItem.UniqueName = project.UniqueName;
                projectItem.Name = project.Name;
                projectItem.FullName = project.FullName;

                if (IsIntegrationServicesProject(project, projectItem))
                {
                    AdjustUniqueNameForExtensionProjects(project, projectItem);
                }

                try
                {
                    projectItem.FullPath = string.IsNullOrWhiteSpace(projectItem.FullName) ? null : Path.GetDirectoryName(projectItem.FullName);
                }
                catch (SystemException ex)
                {
                    // PathTooLongException, ArgumentException (invalid characters).
                    LogManager.ForContext<Solution>().Error(ex, "Failed because path has been to long for {UniqueName}", project?.UniqueName);
                    projectItem.FullPath = null;
                }

                projectItem.SolutionFolder = project.GetTreePath(false) ?? @"\";
                // TODO: SolutionPath = project.GetTreePath(true);
            }
            catch (Exception ex)
            {
                LogManager.ForContext<Solution>().Error(ex, "Failed to update name properties for {UniqueName}", project?.UniqueName);
            }
        }

        private const string _GUID_SQL_INTEGRATIONG_SERVICES_PROJECT_KIND = "{159641d6-6404-4a2a-ae62-294de0fe8301}";

        private static bool IsIntegrationServicesProject(Project project, ProjectItem projectItem)
        {
            return project.Kind == _GUID_SQL_INTEGRATIONG_SERVICES_PROJECT_KIND ? projectItem.UniqueName == projectItem.FullName : false;
        }

        private static void AdjustUniqueNameForExtensionProjects(Project project, ProjectItem projectItem)
        {
            var directory = Path.GetDirectoryName(project.UniqueName);
            var directoryName = Path.GetFileName(directory);
            var csprojName = Path.GetFileName(project.UniqueName);
            projectItem.UniqueName = Path.Combine(directoryName, csprojName);
        }
    }
}
