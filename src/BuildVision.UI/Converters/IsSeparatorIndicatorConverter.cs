using BuildVision.UI.Models.Indicators;
using BuildVision.UI.Models.Indicators.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(ValueIndicator), typeof(bool))]
    public class IsSeparatorIndicatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is SeparatorIndicator);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}