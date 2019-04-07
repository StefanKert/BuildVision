using System.ComponentModel;
using BuildVision.Common;
using BuildVision.UI.Models;

namespace BuildVision.UI.Helpers
{
    public class ProjectItemColumnSorter : PropertyColumnSorter<ProjectItem>
    {
        public ProjectItemColumnSorter(ListSortDirection direction, string propertyName)
            : base(direction, propertyName)
        {
        }
    }
}
