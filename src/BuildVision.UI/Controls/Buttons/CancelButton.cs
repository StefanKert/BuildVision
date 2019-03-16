using System.Windows;
using System.Windows.Controls;

namespace BuildVision.UI.Controls.Buttons
{
    public class CancelButton : Button
    {
        static CancelButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CancelButton), new FrameworkPropertyMetadata(typeof(CancelButton)));
        }
    }
}
