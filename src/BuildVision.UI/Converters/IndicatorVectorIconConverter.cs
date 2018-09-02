using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Extensions;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(ValueIndicator), typeof(ControlTemplate))]
    public class IndicatorVectorIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value is ValueIndicator);
            return VectorResources.TryGet(ValueIndicator.ResourcesUri, value.GetType().Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}