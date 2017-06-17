using System;
using System.Globalization;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjectsReferencesEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i = 0; i < values.Length - 1; i++)
            {
                if (!ReferenceEquals(values[i], values[i + 1]))
                    return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}