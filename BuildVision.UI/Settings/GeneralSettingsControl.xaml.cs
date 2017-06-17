using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BuildVision.UI.Settings
{
    public partial class GeneralSettingsControl : UserControl
    {
        public GeneralSettingsControl()
        {
            InitializeComponent();
        }

        private void HyperlinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}