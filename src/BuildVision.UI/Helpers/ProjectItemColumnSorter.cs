using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using BuildVision.Common;
using BuildVision.Contracts.Exceptions;
using BuildVision.UI.Models;

namespace BuildVision.UI.Helpers
{
    public class ProjectItemColumnSorter : IComparer
    {
        private readonly int _direction;
        private readonly Func<ProjectItem, object> _getProperrty;

        public ProjectItemColumnSorter(ListSortDirection direction, Func<ProjectItem, object> getProperrty)
        {
            _direction = (direction == ListSortDirection.Ascending) ? 1 : -1;
            _getProperrty = getProperrty;
        }

        int IComparer.Compare(object x, object y) => Compare((ProjectItem)x, (ProjectItem)y);

        protected int Compare(ProjectItem x, ProjectItem y)
        {
            var x1 = _getProperrty(x) as IComparable;
            var y1 = _getProperrty(y) as IComparable;

            if (x1 != null && y1 != null)
            {
                return x1.CompareTo(y1) * _direction;
            }

            if (x1 == null && y1 == null)
            {
                return 0;
            }

            // Null values always in the bottom.
            return (x1 == null) ? 1 : -1;
        }
    }
}
