using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Helpers
{
    public static class VsHierarchyExtensions
    {
        public static Project ToProject(this IVsHierarchy pHierProj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            pHierProj.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out var objProj);
            return objProj as Project;
        }
    }
}
