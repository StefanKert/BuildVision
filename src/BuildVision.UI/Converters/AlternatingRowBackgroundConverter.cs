using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BuildVision.UI.Converters
{
    /// <summary>
    /// Converts RowBackground to darker AlternatingRowBackground.
    /// </summary>
    [ValueConversion(typeof(SolidColorBrush), typeof(SolidColorBrush))]
    public class AlternatingRowBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var backg = value as SolidColorBrush;
            if (backg == null)
                return value;

            Color color = backg.Color;

            int darkerDelta = -15;
            int r = color.R + darkerDelta;
            int g = color.G + darkerDelta;
            int b = color.B + darkerDelta;

            int signMinusCount = 0;
            if (r < 0)
            {
                r = 0;
                signMinusCount++;
            }

            if (g < 0)
            {
                g = 0;
                signMinusCount++;
            }

            if (b < 0)
            {
                b = 0;
                signMinusCount++;
            }

            if (signMinusCount > 1)
            {
                int lighterDelta = -darkerDelta;

                r = color.R + lighterDelta;
                g = color.G + lighterDelta;
                b = color.B + lighterDelta;

                if (r > 255) r = 255;
                if (g > 255) g = 255;
                if (b > 255) b = 255;
            }

            var newColor = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
            return new SolidColorBrush(newColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}