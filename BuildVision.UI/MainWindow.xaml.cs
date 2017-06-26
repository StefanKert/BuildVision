using BuildVision.UI.Settings;
using BuildVision.UI.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BuildVision.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var controlViewModel = new ControlViewModel();
            ControlView.DataContext = controlViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(new GeneralSettingsControl());
            settingsWindow.ShowDialog();
        }
    }
}
