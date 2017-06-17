using System.ComponentModel;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting
{
    public class ProjectItemColumnSorter : PropertyColumnSorter<ProjectItem>
    {
        public ProjectItemColumnSorter(ListSortDirection direction, string propertyName)
            : base(direction, propertyName)
        {
        }
    }
}