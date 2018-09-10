using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(SolidColorBrush), typeof(SolidColorBrush))]
    public class InvertSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (SolidColorBrush)value;
            var invertedColor = InvertColour(val.Color);
            return new SolidColorBrush(invertedColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        private static Color InvertColour(Color color)
        {
            return Color.FromArgb(255, (byte)~color.R, (byte)~color.G, (byte)~color.B);
        }
    }
}