using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool normal = (parameter == null);

            var visible = normal ? (bool)value : !(bool)value;
            return visible ? Visibility.Visible : Visibility.Collapsed;  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool normal = (parameter == null);

            var visibility = (Visibility)value;
            return normal ? (visibility == Visibility.Visible) 
                : (visibility != Visibility.Visible);
        }
    }
}