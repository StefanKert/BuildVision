using System;
using System.Globalization;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class NumbersToPercentageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values == null || values.Length != 2)
            {
                throw new InvalidOperationException();
            }

            _ = Double.TryParse(values[0].ToString(), out double currentValue);
            _ = Double.TryParse(values[1].ToString(), out double maxValue);

            if (currentValue > maxValue)
            {
                throw new InvalidOperationException();
            }

            if(maxValue == 0)
            {
                return 0;
            }

            return 100 / maxValue * currentValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
