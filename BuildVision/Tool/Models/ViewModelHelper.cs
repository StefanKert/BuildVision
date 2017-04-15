using System;
using System.IO;
using System.Linq;

using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Helpers;

using EnvDTE;
using System.Collections.Generic;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public static  class ViewModelHelper
    {
        public static void UpdateProperties(Project project, ProjectItem projectItem)
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

        public static void UpdateSolution(Solution solution, SolutionItem solutionItem)
        {
            UpdateSolutionProperties(solutionItem, solution);
        }

        private static void UpdateSolutionProperties(SolutionItem solutionItem, Solution solution)
        {
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
        }

        public static void UpdateProjects(SolutionItem solutionItem, Solution solution)
        {
            solutionItem.AllProjects.Clear();

            if (solution == null)
                return;

            IList<Project> dteProjects;
            try
            {
                dteProjects = solution.GetProjects();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                return;
            }

            var projectItems = new List<ProjectItem>(dteProjects.Count);
            foreach (Project project in dteProjects)
            {
                try
                {
                    var projectItem = new ProjectItem();
                    UpdateProperties(project, projectItem);
                    projectItems.Add(projectItem);
                }
                catch (Exception ex)
                {
                    ex.TraceUnknownException();
                }
            }

            solutionItem.AllProjects.AddRange(projectItems);
        }
    }
}