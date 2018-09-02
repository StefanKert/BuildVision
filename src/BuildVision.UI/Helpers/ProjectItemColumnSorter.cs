using BuildVision.Common;
using BuildVision.UI.Models;
using System.ComponentModel;

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