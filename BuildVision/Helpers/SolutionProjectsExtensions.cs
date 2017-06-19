using System;
using System.Collections.Generic;

using EnvDTE;
using EnvDTE80;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.Runtime.InteropServices;

namespace BuildVision.Helpers
{
    public static class SolutionProjectsExtensions
    {
        public static IList<Project> GetProjects(this Solution solution)
        {            
            var list = new List<Project>();
            foreach(var proj in solution.Projects)
            {
                Project project = proj as Project;
                if (project == null)
                    continue;

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
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

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
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