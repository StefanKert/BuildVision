using BuildVision.UI.Converters;
using BuildVision.UI.DataGrid;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using BuildVision.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace BuildVision.UI
{
    /// <summary>
    /// Interaction logic for ControlView.xaml
    /// </summary>
    public partial class ControlView : UserControl
    {
        private BuildVisionPaneViewModel _viewModel;

        public ControlView()
        {
            InitializeComponent();
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(DataContext != null);

            _viewModel = (BuildVisionPaneViewModel) DataContext;
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO implement logic for scrolling tu currentproject
            //if (_viewModel.CurrentProject != null && e.PropertyName == "CurrentProject")
            //{
            //    // TODO: Remove SelectedIndex = -1 and implement Unselect row feature by clicking on selected row.
            //    Grid.SelectedIndex = -1;

            //    if (Grid.SelectedIndex == -1)
            //        Grid.ScrollIntoView(_viewModel.CurrentProject);
            //}
        }
    }
}
