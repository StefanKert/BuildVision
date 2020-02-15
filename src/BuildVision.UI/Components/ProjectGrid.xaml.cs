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
using BuildVision.UI.Converters;
using BuildVision.UI.DataGrid;
using BuildVision.UI.Helpers;
using BuildVision.UI.ViewModels;

namespace BuildVision.UI.Components
{
    public partial class ProjectGrid : UserControl
    {
        public BuildVisionPaneViewModel ViewModel
        {
            get => (BuildVisionPaneViewModel)DataContext;
            set => DataContext = value;
        }

        public ProjectGrid()
        {
            InitializeComponent();
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);
            Grid.TargetUpdated += GridOnTargetUpdated;
        }

        private void GridOnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property.Name == nameof(Grid.ItemsSource))
            {
                RefreshSortDirectionInGrid();
            }
        }

        private void RefreshSortDirectionInGrid()
        {
            Debug.Assert(Grid.Columns != null);
            var dataGridColumn = Grid.Columns.FirstOrDefault(col => col.GetBindedProperty() == ViewModel.GridSortDescription.Property);
            if (dataGridColumn != null)
            {
                dataGridColumn.SortDirection = ViewModel.GridSortDescription.Order.ToSystem();
            }
        }

        private void GridOnSorting(object sender, DataGridSortingEventArgs e)
        {
            if (ViewModel.GridSorting.CanExecute(e))
            {
                ViewModel.GridSorting.Execute(e);
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(ViewModel != null);
            ViewModel = (BuildVisionPaneViewModel)DataContext;
            ViewModel.SetGridColumnsRef(Grid.Columns);
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO implement logic for scrolling tu currentproject
            if (ViewModel.BuildInformationModel.CurrentProject != null && e.PropertyName == "CurrentProject")
            {
                // TODO: Remove SelectedIndex = -1 and implement Unselect row feature by clicking on selected row.
                Grid.SelectedIndex = -1;

                if (Grid.SelectedIndex == -1)
                {
                    Grid.ScrollIntoView(ViewModel.BuildInformationModel.CurrentProject);
                }
            }
        }

        private void DataGridExpanderOnExpanded(object sender, RoutedEventArgs e)
        {
            ExpanderIsExpandedConverter.SaveState((Expander)sender, false, ViewModel.ControlSettings.GridSettings.CollapsedGroups);
            e.Handled = true;
        }

        private void DataGridExpanderOnCollapsed(object sender, RoutedEventArgs e)
        {
            ExpanderIsExpandedConverter.SaveState((Expander)sender, true, ViewModel.ControlSettings.GridSettings.CollapsedGroups);
            e.Handled = true;
        }

        // Send scrolling to the DataGrid.
        private void DataGridExpanderOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };

            var parent = (UIElement)VisualTreeHelper.GetParent((DependencyObject)sender);
            parent.RaiseEvent(eventArg);
        }

        private void DataGridRowDetailsOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // http://stackoverflow.com/questions/6279724/mousedoubleclick-events-dont-bubble/6326181#6326181)
            if (e.ClickCount == 2)
            {
                e.Handled = true;
            }
        }

        // Autofocus for RowDetails (without extra mouse click).
        private void DataGridRowOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow)sender;
            if (!row.IsSelected && e.Source is DataGridDetailsPresenter)
            {
                row.Focusable = true;
                row.Focus();
                if (Keyboard.FocusedElement is UIElement elementWithFocus)
                {
                    var request = new TraversalRequest(FocusNavigationDirection.Next);
                    elementWithFocus.MoveFocus(request);
                }
            }
        }
    }
}
