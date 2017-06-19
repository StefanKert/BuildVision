using System.Linq;

using EnvDTE;

namespace BuildVision.Helpers
{
    public static class ProjectItemExtensions
    {
        public static bool ProjectItemIsDirty(this ProjectItem projectItem)
        {
            if (projectItem.IsDirty)
                return true;

            if (projectItem.ProjectItems != null && projectItem.ProjectItems.Cast<ProjectItem>().Any(ProjectItemIsDirty))
                return true;

            return false;
        }
    }
}