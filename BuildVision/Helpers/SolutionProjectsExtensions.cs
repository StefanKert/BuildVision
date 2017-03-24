using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;

using EnvDTE;
using EnvDTE80;

using Microsoft.VisualStudio.Shell.Interop;

using Constants = EnvDTE.Constants;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace AlekseyNagovitsyn.BuildVision.Helpers
{
    public static class SolutionProjectsExtensions
    {
        public static IList<Project> GetProjects(this Solution solution, bool withHiddenProjects = false)
        {
            Projects projects = solution.Projects;
            var list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                Project project = item.Current as Project;
                if (project == null)
                    continue;

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    list.AddRange(GetSubProjects(project, withHiddenProjects));
                else if (withHiddenProjects || !ProjectIsHidden(project))
                    list.Add(project);
            }

            return list;
        }

        public static Project GetProject(this Solution solution, Func<Project, bool> cond, bool withHiddenProjects = false)
        {
            Projects projects = solution.Projects;
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                Project project = item.Current as Project;
                if (project == null)
                    continue;

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    Project sub = GetSubProject(project, cond, withHiddenProjects);
                    if (sub != null)
                        return sub;
                }
                else if (withHiddenProjects || !ProjectIsHidden(project))
                {
                    if (cond(project))
                        return project;
                }
            }

            return null;
        }

        public static Project GetSubProject(this Project solutionFolder, Func<Project, bool> cond, bool withHiddenProjects = false)
        {
            for (int i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                Project subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                    continue;

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    Project sub = GetSubProject(subProject, cond, withHiddenProjects);
                    if (sub != null)
                        return sub;
                }
                else if (withHiddenProjects || !ProjectIsHidden(subProject))
                {
                    if (cond(subProject))
                        return subProject;
                }
            }

            return null;
        }

        public static IEnumerable<Project> GetSubProjects(this Project solutionFolder, bool withHiddenProjects = false)
        {
            var list = new List<Project>();
            for (int i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                Project subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                    continue;

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    list.AddRange(GetSubProjects(subProject, withHiddenProjects));
                else if (withHiddenProjects || !ProjectIsHidden(subProject))
                    list.Add(subProject);
            }

            return list;
        }

        /// <summary>
        /// Checks that project is temprorary or internal in Visual Studio.
        /// </summary>
        public static bool ProjectIsHidden(this Project project)
        {
            try
            {
                if (_hiddenProjectsUniqueNames.Contains(project.UniqueName)) 
                    return true;

                // Solution Folder.
                if (project.Kind == Constants.vsProjectKindSolutionItems)
                    return true;

                // If projectIsInitialized == false then NotImplementedException will be occured 
                // in project.FullName or project.FileName getters.
                bool projectIsInitialized = (project.Object != null);
                if (projectIsInitialized && project.FullName.EndsWith(".tmp_proj"))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                return true;
            }
        }

        /// <summary>
        /// Checks that project is temprorary or internal in Visual Studio.
        /// </summary>
        public static bool ProjectIsHidden(string projectFileName)
        {
            try
            {
                if (_hiddenProjectsUniqueNames.Contains(projectFileName))
                    return true;

                if (projectFileName.EndsWith(".tmp_proj"))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                return true;
            }
        }

        private static readonly HashSet<string> _hiddenProjectsUniqueNames = new HashSet<string>
        {
            null,
            string.Empty,
            "<MiscFiles>"
        };

        public static string GetFrameworkString(this Project project)
        {
            try
            {
                var framMonikerProperty = project.GetPropertyOrDefault("TargetFrameworkMoniker");
                if (framMonikerProperty != null)
                {
                    var framMonikerValue = (string)framMonikerProperty.Value;
                    if (string.IsNullOrWhiteSpace(framMonikerValue))
                        return Resources.GridCellNoneText;

                    framMonikerValue = framMonikerValue.Replace(",Version=v", " ");
                    return framMonikerValue;
                }
                else
                {
                    var framVersionProperty = project.GetPropertyOrDefault("TargetFrameworkVersion") 
                                                ?? project.GetPropertyOrDefault("TargetFramework");
                    if (framVersionProperty == null || framVersionProperty.Value == null)
                        return Resources.GridCellNoneText;

                    var version = Convert.ToInt32(framVersionProperty.Value);
                    string versionStr;
                    switch (version)
                    {
                        case 0x10000:
                            versionStr = "1.0";
                            break;
                        case 0x10001:
                            versionStr = "1.1";
                            break;
                        case 0x20000:
                            versionStr = "2.0";
                            break;
                        case 0x30000:
                            versionStr = "3.0";
                            break;
                        case 0x30005:
                            versionStr = "3.5";
                            break;
                        case 0x40000:
                            versionStr = "4.0";
                            break;
                        case 0x40005:
                            versionStr = "4.5";
                            break;

                        case 0:
                            return Resources.GridCellNoneText;

                        default:
                            {
                                try
                                {
                                    string hexVersion = version.ToString("X");
                                    if (hexVersion.Length == 5)
                                    {
                                        int v1 = int.Parse(hexVersion[0].ToString(CultureInfo.InvariantCulture));
                                        int v2 = int.Parse(hexVersion.Substring(1));
                                        versionStr = string.Format("{0}.{1}", v1, v2);
                                    }
                                    else
                                    {
                                        versionStr = string.Format("[0x{0:X}]", version);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.TraceUnknownException();
                                    return Resources.GridCellNAText;
                                }
                            }
                            break;
                    }

                    versionStr = string.Format("Unknown {0}", versionStr);
                    return versionStr;
                }
            }
            catch (ArgumentException)
            {
                return Resources.GridCellNoneText;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                return Resources.GridCellNAText;
            }
        }

        #region EnvDTE.Project tree path (as in solution explorer)

        public static string GetTreePath(this Project project, bool includeSelfProjectName = true)
        {
            var path = new StringBuilder();

            try
            {
                if (includeSelfProjectName)
                    path.Append(project.Name);

                var parent = project;
                while (true)
                {
                    parent = TryGetParentProject(parent);
                    if (parent == null)
                        break;

                    if (path.Length != 0)
                        path.Insert(0, '\\');

                    path.Insert(0, parent.Name);
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }

            return (path.Length != 0) ? path.ToString() : null;
        }

        public static Project TryGetParentProject(this Project project)
        {
            try
            {
                var parentItem = project.ParentProjectItem;
                return (parentItem != null) ? parentItem.ContainingProject : null;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                return null;
            }
        }

        #endregion

        #region Get the project flavor (subtype) of a EnvDTE.Project

        public static string GetProjectTypeGuids(this Project proj)
        {
            string projectTypeGuids = string.Empty;

            var service = GetService(proj.DTE, typeof(IVsSolution));
            var solution = (IVsSolution)service;

            IVsHierarchy hierarchy;
            int result = solution.GetProjectOfUniqueName(proj.UniqueName, out hierarchy);

            if (result == 0)
            {
                var aggregatableProject = (IVsAggregatableProject)hierarchy;
                result = aggregatableProject.GetAggregateProjectTypeGuids(out projectTypeGuids);
            }

            return projectTypeGuids;
        }

        public static object GetService(object serviceProvider, Type type)
        {
            return GetService(serviceProvider, type.GUID);
        }

        public static object GetService(object serviceProviderObject, Guid guid)
        {
            object service = null;
            IntPtr serviceIntPtr;

            Guid sidGuid = guid;
            Guid iidGuid = sidGuid;
            var serviceProvider = (IServiceProvider)serviceProviderObject;
            int hr = serviceProvider.QueryService(ref sidGuid, ref iidGuid, out serviceIntPtr);

            if (hr != 0)
            {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            }
            else if (!serviceIntPtr.Equals(IntPtr.Zero))
            {
                service = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(serviceIntPtr);
                System.Runtime.InteropServices.Marshal.Release(serviceIntPtr);
            }

            return service;
        }

        #endregion

        #region Get property

        public static Property GetPropertyOrDefault(this Properties properties, string propertyName)
        {
            try
            {
                return properties.Item(propertyName);
            }
            catch (ArgumentException)
            {
                // not found.
                return null;
            }
        }

        public static T GetPropertyOrDefault<T>(this Properties properties, string propertyName)
            where T : class
        {
            var property = GetPropertyOrDefault(properties, propertyName);
            if (property == null)
                return null;

            return (T)property.Value;
        }

        public static object TryGetPropertyValueOrDefault(this Properties properties, string propertyName)
        {
            try
            {
                return properties.Item(propertyName).Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static T TryGetPropertyValueOrDefault<T>(this Properties properties, string propertyName)
            where T : class
        {
            return TryGetPropertyValueOrDefault(properties, propertyName) as T;
        }

        public static Property GetPropertyOrDefault(this Project project, string propertyName)
        {
            return GetPropertyOrDefault(project.Properties, propertyName);
        }

        public static object TryGetPropertyValueOrDefault(this Project project, string propertyName)
        {
            return TryGetPropertyValueOrDefault(project.Properties, propertyName);
        }

        public static T TryGetPropertyValueOrDefault<T>(this Project project, string propertyName) 
            where T : class
        {
            return TryGetPropertyValueOrDefault<T>(project.Properties, propertyName);
        }

        #endregion

        public static string GetTargetAssemblyPath(this Project project, string configuration = null, string platform = null)
        {
            Configuration targetConfig;
            try
            {
                if (configuration != null && platform != null)
                    targetConfig = project.ConfigurationManager.Item(configuration, platform);
                else
                    targetConfig = project.ConfigurationManager.ActiveConfiguration;  
            }
            catch (ArgumentException ex)
            {
                throw new Exception(string.Format("Configuration not found: {0} {1}", configuration, platform), ex);
            }

            try
            {
                string fullPath = project.Properties.Item("FullPath").Value.ToString();
                string outputPath = targetConfig.Properties.Item("OutputPath").Value.ToString();
                string outputDir = Path.Combine(fullPath, outputPath);
                string outputFileName = project.Properties.Item("OutputFileName").Value.ToString();
                string assemblyPath = Path.Combine(outputDir, outputFileName);
                return assemblyPath;
            }
            catch (ArgumentException)
            {
                // Item not found in Properties collection.
                return null;
            }
        }

        public static IEnumerable<string> GetBuildOutputFilePaths(
            this Project project, 
            BuildOutputFileTypes fileTypes, 
            string configuration = null, 
            string platform = null)
        {
            Configuration targetConfig;
            if (configuration != null && platform != null)
                targetConfig = project.ConfigurationManager.Item(configuration, platform);
            else
                targetConfig = project.ConfigurationManager.ActiveConfiguration;

            var groups = new List<string>();
            if (fileTypes.LocalizedResourceDlls)
                groups.Add(BuildOutputGroup.LocalizedResourceDlls);
            if (fileTypes.XmlSerializer)
                groups.Add("XmlSerializer");
            if (fileTypes.ContentFiles)
                groups.Add(BuildOutputGroup.ContentFiles);
            if (fileTypes.Built)
                groups.Add(BuildOutputGroup.Built);
            if (fileTypes.SourceFiles)
                groups.Add(BuildOutputGroup.SourceFiles);
            if (fileTypes.Symbols)
                groups.Add(BuildOutputGroup.Symbols);
            if (fileTypes.Documentation)
                groups.Add(BuildOutputGroup.Documentation);

            var filePaths = new List<string>();
            foreach (string groupName in groups)
            {
                try
                {
                    var group = targetConfig.OutputGroups.Item(groupName);
                    var fileUrls = (object[])group.FileURLs;
                    filePaths.AddRange(fileUrls.Select(x => new Uri(x.ToString()).LocalPath));
                }
                catch (ArgumentException ex)
                {
                    var msg = string.Format("Build Output Group \"{0}\" not found (Project Kind is \"{1}\").", groupName, project.Kind);
                    ex.Trace(msg, EventLogEntryType.Warning);
                }
            }

            return filePaths.Distinct();
        }

        /// <summary>
        /// Find file in the Project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="filePath">File path, relative to the <paramref name="project"/> root.</param>
        /// <returns>The found file or <c>null</c>.</returns>
        private static ProjectItem FindProjectItem(this Project project, string filePath)
        {
            return FindProjectItem(project.ProjectItems, filePath);
        }

        /// <summary>
        /// Find file in projects collection.
        /// </summary>
        /// <param name="items">Projects collection.</param>
        /// <param name="filePath">File path, relative to the <paramref name="items"/> root.</param>
        /// <returns>The found file or <c>null</c>.</returns>
        private static ProjectItem FindProjectItem(this ProjectItems items, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Argument `filePath` is null or empty.", "filePath");

            int backslashIndex = filePath.IndexOf("\\", StringComparison.Ordinal);
            bool findFolder = (backslashIndex != -1);
            if (findFolder)
            {
                string folderName = filePath.Substring(0, backslashIndex);
                foreach (ProjectItem item in items)
                {
                    if (item.Kind != Constants.vsProjectItemKindVirtualFolder &&
                        item.Kind != Constants.vsProjectItemKindPhysicalFolder)
                        continue;

                    if (folderName == item.Name)
                    {
                        string nextpath = filePath.Substring(backslashIndex + 1);
                        return FindProjectItem(item.ProjectItems, nextpath);
                    }
                }
            }
            else
            {
                string fileName = filePath;
                foreach (ProjectItem item in items)
                {
                    if (item.Kind != Constants.vsProjectItemKindPhysicalFile) 
                        continue;

                    if (item.Name == fileName)
                        return item;

                    // Nested item, e.g. Default.aspx or MainWindow.xaml.
                    if (item.ProjectItems.Count > 0)
                    {
                        ProjectItem childItem = FindProjectItem(item.ProjectItems, fileName);
                        if (childItem != null)
                            return childItem;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Navigate to the Error Item in the Visual Studio Editor.
        /// </summary>
        /// <param name="project">The project - owner of the Error Item.</param>
        /// <param name="errorItem">The Error Item.</param>
        public static bool NavigateToErrorItem(this Project project, Tool.Building.ErrorItem errorItem)
        {
            try
            {
                if (project == null)
                    throw new ArgumentNullException("project");

                if (errorItem == null)
                    throw new ArgumentNullException("errorItem");

                if (!errorItem.CanNavigateTo)
                    return false;

                if (string.IsNullOrEmpty(errorItem.File))
                    return false;

                string fullPath;

                // Check if path is absolute.
                if (Path.IsPathRooted(errorItem.File))
                {
                    // Foo VC++ projects errorItem.File contains full path.
                    fullPath = errorItem.File;
                }
                else
                {
                    var projectItemFile = project.FindProjectItem(errorItem.File);
                    if (projectItemFile == null)
                        return false;

                    fullPath = projectItemFile.Properties.GetPropertyOrDefault<string>("FullPath");
                    if (fullPath == null)
                        throw new KeyNotFoundException("FullPath property not found.");
                }

                try
                {
                    var window = project.DTE.ItemOperations.OpenFile(fullPath, Constants.vsViewKindAny);
                    if (window == null)
                        throw new NullReferenceException("Associated window is null reference.");

                    window.Activate();

                    var selection = (TextSelection)window.Selection;
                    selection.MoveToLineAndOffset(errorItem.LineNumber, errorItem.ColumnNumber);
                    selection.MoveToLineAndOffset(errorItem.EndLineNumber, errorItem.EndColumnNumber, true /*select text*/);
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Navigate to error item exception (fullPath='{0}').", fullPath);
                    ex.Trace(msg);
                }
            }
            catch (Exception ex)
            {
                ex.Trace("Navigate to error item exception.");
            }

            return true;
        }
    }
}