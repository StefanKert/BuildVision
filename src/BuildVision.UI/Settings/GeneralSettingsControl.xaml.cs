using BuildVision.Common;
using BuildVision.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
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
            GithubHelper.OpenBrowserWithPrefilledIssue();
            e.Handled = true;
        }
    }
}