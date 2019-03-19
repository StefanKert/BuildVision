using System.Windows;

namespace BuildVision.UI.Controls.Indicators
{
    public class SuccessProjectsIndicator : ValueIndicator
    {
        static SuccessProjectsIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuccessProjectsIndicator), new FrameworkPropertyMetadata(typeof(SuccessProjectsIndicator)));
        }

        public SuccessProjectsIndicator()
        {
            Header = UI.Resources.SuccessProjectsIndicator_Header;
        }
    }
}
