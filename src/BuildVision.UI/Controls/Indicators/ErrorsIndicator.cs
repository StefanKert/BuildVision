using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class ErrorsIndicator : ValueIndicator
    {
        static ErrorsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ErrorsIndicator), new FrameworkPropertyMetadata(typeof(ErrorsIndicator)));
        }
    }
}
