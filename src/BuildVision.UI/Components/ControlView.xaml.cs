using System.Globalization;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BuildVision.UI
{
    /// <summary>
    /// Interaction logic for ControlView.xaml
    /// </summary>
    public partial class ControlView : UserControl
    {
        public ControlView()
        {
            InitializeComponent();
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }
    }
}
