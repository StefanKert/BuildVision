using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Buttons
{
    public class BuildSolutionButton : Button
    {
        static BuildSolutionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BuildSolutionButton), new FrameworkPropertyMetadata(typeof(BuildSolutionButton)));
        }
    }
}
