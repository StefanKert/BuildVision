using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using BuildVision.Contracts;
using BuildVision.UI.Extensions;
using BuildVision.UI.Models;

namespace BuildVision.UI.Converters
{
    [ValueConversion(typeof(string), typeof(ControlTemplate))]
    public class StateIconKeyToIconConverter : IValueConverter
    {
        public const string ResourcesUri = @"Resources/BuildState.Resources.xaml";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stateIconKey = (string)value;
            var vector =  VectorResources.TryGet(ResourcesUri, stateIconKey) ?? VectorResources.TryGet(ResourcesUri, "StandBy");
            return vector;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // hope this works.. we don´t need back conversion I guess
            return value;
        }
    }
}

