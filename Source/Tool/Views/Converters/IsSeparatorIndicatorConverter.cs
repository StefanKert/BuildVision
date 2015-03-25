using System;
using System.Globalization;
using System.Windows.Data;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Converters
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