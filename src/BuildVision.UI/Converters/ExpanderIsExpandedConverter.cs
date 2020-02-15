using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    public class ExpanderIsExpandedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var collectionViewGroup = (CollectionViewGroup)values[0];

            if (!(values[1] is IList<string> collapsedGroups) || collapsedGroups.Count == 0)
            {
                return true;
            }

            string groupId = GetGroupIdentifier(collectionViewGroup);
            bool collapsed = collapsedGroups.Contains(groupId);
            return !collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        public static void SaveState(Expander exp, bool collapsed, IList<string> collapsedGroups)
        {
            if (!(exp.DataContext is CollectionViewGroup collectionViewGroup))
            {
                return;
            }

            string groupId = GetGroupIdentifier(collectionViewGroup);
            if (collapsed)
            {
                if (!collapsedGroups.Contains(groupId))
                {
                    collapsedGroups.Add(groupId);
                }
            }
            else
            {
                collapsedGroups.Remove(groupId);
            }
        }

        private static string GetGroupIdentifier(CollectionViewGroup collectionViewGroup)
        {
            object groupId = collectionViewGroup.Name;
            if (groupId == null)
            {
                return null;
            }

            return groupId is string ? (string)groupId : groupId.ToString();
        }
    }
}
