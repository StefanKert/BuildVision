using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class WarningsIndicator : ValueIndicator
    {
        static WarningsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WarningsIndicator), new FrameworkPropertyMetadata(typeof(WarningsIndicator)));
        }

        public WarningsIndicator()
        {
            Header = UI.Resources.WarningsIndicator_Header;
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long)e.NewValue);
        }
    }
}
