using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;
using BuildVision.UI.ViewModels;

namespace BuildVision.UI
{
    /// <summary>
    /// Interaction logic for ControlView.xaml
    /// </summary>
    public partial class ControlView : UserControl
    {
        public BuildVisionPaneViewModel ViewModel
        {
            get => (BuildVisionPaneViewModel)DataContext;
            set => DataContext = value;
        }

        public ControlView()
        {
            InitializeComponent();
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                ViewModel = new BuildVisionPaneViewModel();
            }
        }
    }
}
