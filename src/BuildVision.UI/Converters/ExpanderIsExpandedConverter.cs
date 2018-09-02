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
            var collapsedGroups = (IList<string>)values[1];

            if (collapsedGroups == null || collapsedGroups.Count == 0)
                return true;

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
            var collectionViewGroup = exp.DataContext as CollectionViewGroup;
            if (collectionViewGroup == null)
                return;

            string groupId = GetGroupIdentifier(collectionViewGroup);
            if (collapsed)
            {
                if (!collapsedGroups.Contains(groupId))
                    collapsedGroups.Add(groupId);
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
                return null;

            if (groupId is string)
                return (string)groupId;

            return groupId.ToString();
        }
    }
}
