using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class MessagesIndicator : ValueIndicator
    {
        static MessagesIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessagesIndicator), new FrameworkPropertyMetadata(typeof(MessagesIndicator)));
        }

        public MessagesIndicator()
        {
            Header = UI.Resources.MessagesIndicator_Header;
        }

        static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(ValueProperty, (long)e.NewValue);
        }
    }
}
