using System;
using System.Globalization;
using System.Windows.Data;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class ParameterIsEqualsValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}