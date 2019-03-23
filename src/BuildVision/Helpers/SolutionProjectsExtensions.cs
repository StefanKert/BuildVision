using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BuildVision.Core;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using EnvDTE;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using ProjectItem = BuildVision.UI.Models.ProjectItem;

namespace BuildVision.Helpers
{
    public static class SolutionProjectsExtensions
    {
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
                ex.TraceUnknownException();

                solutionItem.Name = Resources.GridCellNATextInBrackets;
                solutionItem.FullName = Resources.GridCellNATextInBrackets;
                solutionItem.IsEmpty = true;
            }
            return solutionItem;
        }

        public static IList<Project> GetProjects(this Solution solution)
        {            
            var list = new List<Project>();
            foreach(var proj in solution.Projects)
            {
                Project project = proj as Project;
                if (project == null)
                    continue;

                if (project.Kind == EnvDTEProjectKinds.ProjectKindSolutionFolder)
                    list.AddRange(project.GetSubProjects());
                else if (!project.IsHidden())
                    list.Add(project);
            }
            return list;
        }

        public static Project GetProject(this Solution solution, Func<Project, bool> cond)
        {
            Projects projects = solution.Projects;
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                Project project = item.Current as Project;
                if (project == null)
                    continue;

                if (project.Kind == EnvDTEProjectKinds.ProjectKindSolutionFolder)
                {
                    Project sub = project.GetSubProject(cond);
                    if (sub != null)
                        return sub;
                }
                else if (!project.IsHidden())
                {
                    if (cond(project))
                        return project;
                }
            }

            return null;
        }

        public static IList<ProjectItem> GetProjectItems(this Solution solution)
        {
            IList<Project> dteProjects = new List<Project>();
            try
            {
                dteProjects = solution.GetProjects();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }


            var projectItems = new List<ProjectItem>(dteProjects.Count);
            foreach (Project project in dteProjects)
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
                    ex.TraceUnknownException();
                }
            }

            return projectItems;
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
                    ex.TraceUnknownException();
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
                    projectItem.CommonType = ProjectExtensions.GetProjectType(project.Kind, project.DTE.Version /* "12.0" */);
                }
                catch (Exception ex)
                {
                    ex.TraceUnknownException();
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
                        config = project.ConfigurationManager.ActiveConfiguration;
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                        config = null;
                    }

                    if (config != null)
                    {
                        projectItem.Configuration = config.ConfigurationName;
                        projectItem.Platform = config.PlatformName;
                    }
                    else
                    {
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
                    ex.TraceUnknownException();
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
                    AdjustUniqueNameForExtensionProjects(project, projectItem);

                try
                {
                    projectItem.FullPath = string.IsNullOrWhiteSpace(projectItem.FullName) ? null : Path.GetDirectoryName(projectItem.FullName);
                }
                catch (SystemException ex)
                {
                    // PathTooLongException, ArgumentException (invalid characters).
                    ex.TraceUnknownException();
                    projectItem.FullPath = null;
                }

                projectItem.SolutionFolder = project.GetTreePath(false) ?? @"\";
                // TODO: SolutionPath = project.GetTreePath(true);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private const string _GUID_SQL_INTEGRATIONG_SERVICES_PROJECT_KIND = "{159641d6-6404-4a2a-ae62-294de0fe8301}";

        private static bool IsIntegrationServicesProject(Project project, ProjectItem projectItem)
        {
            if (project.Kind == _GUID_SQL_INTEGRATIONG_SERVICES_PROJECT_KIND)
                return projectItem.UniqueName == projectItem.FullName;
            else
                return false;
        }

        private static void AdjustUniqueNameForExtensionProjects(Project project, ProjectItem projectItem)
        {
            var directory = Path.GetDirectoryName(project.UniqueName);
            var directoryName = Path.GetFileName(directory);
            var csprojName = Path.GetFileName(project.UniqueName);
            projectItem.UniqueName = Path.Combine(directoryName, csprojName);
        }

        public static object GetService(object serviceProviderObject, Type type)
        {
            object service = null;
            IntPtr serviceIntPtr;

            Guid sidGuid = type.GUID;
            Guid iidGuid = sidGuid;
            var serviceProvider = (IServiceProvider)serviceProviderObject;
            int hr = serviceProvider.QueryService(ref sidGuid, ref iidGuid, out serviceIntPtr);

            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            else if (!serviceIntPtr.Equals(IntPtr.Zero))
            {
                service = Marshal.GetObjectForIUnknown(serviceIntPtr);
                Marshal.Release(serviceIntPtr);
            }

            return service;
        }
    }
}
