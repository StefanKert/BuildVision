using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns;
using AlekseyNagovitsyn.BuildVision.Tool.ViewModels;
using AlekseyNagovitsyn.BuildVision.Tool.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            var settingsWindow = new SettingsWindow(new BuildMessagesSettingsControl());
            settingsWindow.ShowDialog();
        }
    }
}
