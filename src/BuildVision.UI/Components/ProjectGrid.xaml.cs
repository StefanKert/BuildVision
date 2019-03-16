using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BuildVision.UI.Converters;
using BuildVision.UI.ViewModels;

namespace BuildVision.UI.Components
{
    /// <summary>
    /// Interaction logic for ProjectGrid.xaml
    /// </summary>
    public partial class ProjectGrid : UserControl
    {
        private BuildVisionPaneViewModel _viewModel;

        public ProjectGrid()
        {
            InitializeComponent();
        }

        private void GridOnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property.Name == "ItemsSource")
                RefreshSortDirectionInGrid();
        }

        private void RefreshSortDirectionInGrid()
        {
            //DataGridColumn dataGridColumn = Grid.Columns.FirstOrDefault(col => col.GetBindedProperty() == _viewModel.GridSortDescription.Property);
            //if (dataGridColumn != null)
            //    dataGridColumn.SortDirection = _viewModel.GridSortDescription.Order.ToSystem();
        }

        private void GridOnSorting(object sender, DataGridSortingEventArgs e)
        {
            if (_viewModel.GridSorting.CanExecute(e))
                _viewModel.GridSorting.Execute(e);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Debug.Assert(DataContext != null);

            _viewModel = (BuildVisionPaneViewModel) DataContext;
            //_viewModel.SetGridColumnsRef(Grid.Columns);
            //_viewModel.PropertyChanged += ViewModelOnPropertyChanged;
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

        private void DataGridExpanderOnExpanded(object sender, RoutedEventArgs e)
        {
            ExpanderIsExpandedConverter.SaveState(
                (Expander) sender,
                false,
                _viewModel.ControlSettings.GridSettings.CollapsedGroups);
            e.Handled = true;
        }

        private void DataGridExpanderOnCollapsed(object sender, RoutedEventArgs e)
        {
            ExpanderIsExpandedConverter.SaveState(
                (Expander) sender,
                true,
                _viewModel.ControlSettings.GridSettings.CollapsedGroups);
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

            var parent = (UIElement) VisualTreeHelper.GetParent((DependencyObject) sender);
            parent.RaiseEvent(eventArg);
        }

        private void DataGridRowDetailsOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // http://stackoverflow.com/questions/6279724/mousedoubleclick-events-dont-bubble/6326181#6326181)
            if (e.ClickCount == 2)
                e.Handled = true;
        }

        // Autofocus for RowDetails (without extra mouse click).
        private void DataGridRowOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow) sender;
            if (!row.IsSelected && e.Source is DataGridDetailsPresenter)
            {
                row.Focusable = true;
                row.Focus();

                // Gets the element with keyboard focus.
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                // Change keyboard focus.
                if (elementWithFocus != null)
                {
                    var request = new TraversalRequest(FocusNavigationDirection.Next);
                    elementWithFocus.MoveFocus(request);
                }
            }
        }
    }
}
