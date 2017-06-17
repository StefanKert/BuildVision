using BuildVision.Contracts;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Extensions;
using BuildVision.UI.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BuildVision.UI
{
    /// <summary>
    /// Interaction logic for ErrorsGrid.xaml
    /// </summary>
    public partial class ErrorsGrid : UserControl
    {
        private ScrollViewer _errorsGridScrollViewer;

        public ErrorsGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProjectItemProperty = DependencyProperty.Register(nameof(ProjectItem), typeof(ProjectItem), typeof(ErrorsGrid));

        public ProjectItem ProjectItem
        {
            get => (ProjectItem)GetValue(ProjectItemProperty);
            set => SetValue(ProjectItemProperty, value);
        }

        private void ErrorsGridRowOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var row = (DataGridRow)sender;
                var errorItem = (ErrorItem)row.Item;
                errorItem.GoToError();
                e.Handled = true;
            }
            catch (Exception ex)
            {
                ex.Trace("Navigate to error item exception.");
            }
        }

        // Send scrolling to the parent DataGrid.
        private void ErrorsGridOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_errorsGridScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = (UIElement)VisualTreeHelper.GetParent((DependencyObject)sender);
            parent.RaiseEvent(eventArg);
        }


        private void ErrorsGridLoaded(object sender, RoutedEventArgs e)
        {
            _errorsGridScrollViewer = VisualHelper.FindVisualChild<ScrollViewer>(this);
            Debug.Assert(_errorsGridScrollViewer != null);
        }
    }
}
