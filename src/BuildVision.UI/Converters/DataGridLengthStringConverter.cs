using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(DataGridLength), typeof(string))]
    public class DataGridLengthStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (DataGridLength)value;

            if (val.IsAuto)
                return "auto";            

            if (val.IsStar)
                return val.DisplayValue.ToString("0.0") + "*";

            return val.Value.ToString("0.0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var val = ((string)value).Trim().ToLower();

                if (val == "auto")
                    return new DataGridLength(1.0, DataGridLengthUnitType.Auto);

                if (val.EndsWith("*"))
                    return new DataGridLength(double.Parse(val.TrimEnd('*')), DataGridLengthUnitType.Star);

                return new DataGridLength(double.Parse(val));
            }
            catch (Exception)
            {
                return new DataGridLength(1.0, DataGridLengthUnitType.Auto);
            }
        }
    }
}