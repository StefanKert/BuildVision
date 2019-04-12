using System.Windows.Controls;
using System.Windows.Navigation;
using BuildVision.Helpers;

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
            GithubHelper.OpenBrowserWithPrefilledIssue();
            e.Handled = true;
        }
    }
}
