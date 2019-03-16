using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class WarningProjectsIndicator : ValueIndicator
    {
        static WarningProjectsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WarningProjectsIndicator), new FrameworkPropertyMetadata(typeof(WarningProjectsIndicator)));
        }
    }
}
