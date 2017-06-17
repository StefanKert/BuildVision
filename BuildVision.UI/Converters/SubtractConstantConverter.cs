using System;
using System.Globalization;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class SubtractConstantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double originalValue = (double)value;
            double subtractValue = System.Convert.ToDouble(parameter);
            return originalValue - subtractValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}