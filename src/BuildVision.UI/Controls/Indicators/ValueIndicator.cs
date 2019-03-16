using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Indicators
{
    public class ValueIndicator : Control
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", 
            typeof(long), 
            typeof(ValueIndicator), 
            new FrameworkPropertyMetadata(
                defaultValue: (long)-1,
                flags: 
                    FrameworkPropertyMetadataOptions.AffectsArrange 
                    | FrameworkPropertyMetadataOptions.AffectsMeasure 
                    |FrameworkPropertyMetadataOptions.AffectsRender, 
                    propertyChangedCallback: new PropertyChangedCallback(OnValueChange)));

        static ValueIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValueIndicator), new FrameworkPropertyMetadata(typeof(ValueIndicator)));
        }

        public long Value
        {
            get { return (long) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long) e.NewValue);
        }
    }
}
