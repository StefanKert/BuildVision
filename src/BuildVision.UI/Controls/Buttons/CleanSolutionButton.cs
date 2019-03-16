using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Buttons
{
    public class CleanSolutionButton : Button
    {
        static CleanSolutionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CleanSolutionButton), new FrameworkPropertyMetadata(typeof(CleanSolutionButton)));
        }
    }
}
