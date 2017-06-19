using System;
using System.Globalization;
using System.Windows.Data;

using BuildVision.UI;
using BuildVision.UI.DataGrid;
using BuildVision.UI.Settings.Models.Columns;

namespace BuildVision.UI.Converters
{
    public class GridColumnSettingsToColumnExampleValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var gridColumnSettings = (GridColumnSettings)values[0];
            string valueStringFormat = (string)values[1];

            object exampleValue = ColumnsManager.GetColumnExampleValue(gridColumnSettings);
            string example = FormatExample(exampleValue, valueStringFormat);
            return example;
        }

        private static string FormatExample(object example, string stringFormat)
        {
            if (example == null)
                return Resources.GridCellNoneTextInBrackets;

            try
            {
                if (string.IsNullOrWhiteSpace(stringFormat))
                    return example.ToString();

                if (example is DateTime)
                    return ((DateTime)example).ToString(stringFormat);

                if (example is TimeSpan)
                    return ((TimeSpan)example).ToString(stringFormat);

                return string.Format(stringFormat, example);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}