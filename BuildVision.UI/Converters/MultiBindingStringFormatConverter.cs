using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BuildVision.Common;
using BuildVision.UI.Common.Logging;

namespace BuildVision.UI.Converters
{
    /// <summary>
    /// Returns value of string.Format(values[0], values[1..]).
    /// </summary>
    [ValueConversion(typeof(object[]), typeof(string))]
    public class MultiBindingStringFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return string.Empty;

            try
            {
                var frmt = (string)values[0];
                return string.Format(new CustomStringFormatProvider(), frmt, values.Skip(1).ToArray());
            }
            catch (Exception ex)
            {
                ex.Trace("Format error: " + ex.Message);
                return string.Format("<Format error: {0}>", ex.Message);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}