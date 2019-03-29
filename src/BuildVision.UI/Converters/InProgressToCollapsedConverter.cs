using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BuildVision.Contracts;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(BuildState), typeof(Visibility))]
    public class InProgressToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var buildState = (BuildState) value;
            return buildState == BuildState.InProgress ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
