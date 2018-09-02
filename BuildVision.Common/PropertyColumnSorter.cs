using BuildVision.Contracts;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace BuildVision.Common
{
    public class PropertyColumnSorter<T> : IComparer
        where T : class
    {
        private readonly int _direction;
        private readonly PropertyInfo _propertyInfo;

        public PropertyColumnSorter(ListSortDirection direction, string propertyName)
        {
            _direction = (direction == ListSortDirection.Ascending) ? 1 : -1;
            _propertyInfo = typeof(T).GetProperty(propertyName);

            if (_propertyInfo == null)
                throw new PropertyNotFoundException(propertyName, typeof(T));
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare((T) x, (T) y);
        }

        protected int Compare(T x, T y)
        {
            var x1 = _propertyInfo.GetValue(x, null) as IComparable;
            var y1 = _propertyInfo.GetValue(y, null) as IComparable;

            if (x1 != null && y1 != null)
                return x1.CompareTo(y1) * _direction;

            if (x1 == null && y1 == null)
                return 0;

            // Null values always in the bottom.
            return (x1 == null) ? 1 : -1;
        }
    }
}
