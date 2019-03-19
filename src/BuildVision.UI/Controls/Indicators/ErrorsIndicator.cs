using System.Windows;
using BuildVision.UI.Extensions;

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
    }
}
