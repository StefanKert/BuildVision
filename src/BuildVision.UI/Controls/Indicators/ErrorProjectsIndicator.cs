using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Indicators
{
    public class ErrorProjectsIndicator : ValueIndicator
    {
        static ErrorProjectsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorProjectsIndicator), new FrameworkPropertyMetadata(typeof(ErrorProjectsIndicator)));
        }

        public ErrorProjectsIndicator()
        {
            Header = UI.Resources.ErrorProjectsIndicator_Header;
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long)e.NewValue);
        }
    }
}
