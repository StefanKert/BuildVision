using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly Func<ProjectItem, object> _getProperty;

        public ProjectItemColumnSorter(ListSortDirection direction, Func<ProjectItem, object> getProperty)
        {
            _direction = (direction == ListSortDirection.Ascending) ? 1 : -1;
            _getProperty = getProperty;
        }

        int IComparer.Compare(object x, object y) => Compare((ProjectItem)x, (ProjectItem)y);

        protected int Compare(ProjectItem x, ProjectItem y)
        {
            var x1 = _getProperty(x) as IComparable;
            var y1 = _getProperty(y) as IComparable;

   
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
