using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class MessagesIndicator : ValueIndicator
    {
        static MessagesIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessagesIndicator), new FrameworkPropertyMetadata(typeof(MessagesIndicator)));
        }
    }
}
