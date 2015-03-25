using System;
using System.Globalization;
using System.Windows.Data;

using AlekseyNagovitsyn.BuildVision.Tool.DataGrid;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views.Converters
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