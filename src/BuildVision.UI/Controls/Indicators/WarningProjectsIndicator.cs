using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class WarningProjectsIndicator : ValueIndicator
    {
        static WarningProjectsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WarningProjectsIndicator), new FrameworkPropertyMetadata(typeof(WarningProjectsIndicator)));
        }

        public WarningProjectsIndicator()
        {
            Header = UI.Resources.WarningProjectsIndicator_Header;
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long)e.NewValue);
        }
    }
}
