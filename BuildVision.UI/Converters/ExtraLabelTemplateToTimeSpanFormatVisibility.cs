using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using BuildVision.UI.Models;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(BuildExtraMessageFormat), typeof(Visibility))]
    public class ExtraLabelTemplateToTimeSpanFormatVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (BuildExtraMessageFormat)value;
            return (val == BuildExtraMessageFormat.Custom) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}