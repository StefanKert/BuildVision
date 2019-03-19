using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BuildVision.UI.Extensions;

namespace BuildVision.UI.Controls.Indicators
{
    public class ValueIndicator : Control
    {
        public const string ResourcesUri = @"Resources/ValueIndicator.Resources.xaml";

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ValueIndicator));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(ControlTemplate), typeof(ValueIndicator), new PropertyMetadata(null));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), 
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

        public ValueIndicator()
        {
            Icon = VectorResources.TryGet(ResourcesUri, $"{GetType().Name}Icon");
        }

        public long Value
        {
            get { return (long) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public ControlTemplate Icon
        {
            get { return (ControlTemplate) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long) e.NewValue);
        }
    }
}
