using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(DataGridHeadersVisibility), typeof(bool))]
    public class GridColumnHeadersVisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (DataGridHeadersVisibility)value;
            return (visibility == DataGridHeadersVisibility.Column || visibility == DataGridHeadersVisibility.All);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (bool)value;
            return visible ? DataGridHeadersVisibility.Column : DataGridHeadersVisibility.None;
        }
    }
}