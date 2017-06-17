using System;
using System.Globalization;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(double), typeof(bool))]
    public class NumberToIsPositiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val;
            if (value != null && double.TryParse(value.ToString(), out val))
                return val > 0;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}