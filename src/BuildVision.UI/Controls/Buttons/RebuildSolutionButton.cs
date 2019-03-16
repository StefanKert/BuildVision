using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Buttons
{
    public class RebuildSolutionButton : Button
    {
        static RebuildSolutionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RebuildSolutionButton), new FrameworkPropertyMetadata(typeof(RebuildSolutionButton)));
        }
    }
}
