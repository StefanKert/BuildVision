using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Buttons
{
    public class BuildActionButton : Button
    {
        static BuildActionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BuildActionButton), new FrameworkPropertyMetadata(typeof(BuildActionButton)));
        }
    }
}
