using System;
using System.Globalization;
using System.Windows.Data;

using BuildVision.UI.DataGrid;
using BuildVision.UI.Settings.Models.Columns;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(GridColumnSettings), typeof(string))]
    public class GridColumnSettingsToInitialColumnHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ColumnsManager.GetInitialColumnHeader((GridColumnSettings)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}