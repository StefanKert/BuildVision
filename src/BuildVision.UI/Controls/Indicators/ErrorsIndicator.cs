using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class ErrorsIndicator : ValueIndicator
    {
        static ErrorsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorsIndicator), new FrameworkPropertyMetadata(typeof(ErrorsIndicator)));
        }

        public ErrorsIndicator()
        {
            Header = UI.Resources.ErrorsIndicator_Header;
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long)e.NewValue);
        }
    }
}
