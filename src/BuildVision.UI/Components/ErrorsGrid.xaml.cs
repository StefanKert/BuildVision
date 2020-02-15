using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BuildVision.Common.Logging;
using BuildVision.Contracts;
using BuildVision.UI.Extensions;
using BuildVision.UI.Models;

namespace BuildVision.UI
{
    /// <summary>
    /// Interaction logic for ErrorsGrid.xaml
    /// </summary>
    public partial class ErrorsGrid : UserControl
    {
        public static readonly DependencyProperty ProjectItemProperty = DependencyProperty.Register(nameof(ProjectItem), typeof(ProjectItem), typeof(ErrorsGrid));
        public static DependencyProperty NavigateToErrorCommandProperty = DependencyProperty.Register(nameof(NavigateToErrorCommand), typeof(ICommand), typeof(ErrorsGrid));

        private ScrollViewer _errorsGridScrollViewer;

        public ErrorsGrid()
        {
            InitializeComponent();
        }


        public ProjectItem ProjectItem
        {
            get => (ProjectItem)GetValue(ProjectItemProperty);
            set => SetValue(ProjectItemProperty, value);
        }

        public ICommand NavigateToErrorCommand
        {
            get => (ICommand)GetValue(NavigateToErrorCommandProperty);
            set => SetValue(NavigateToErrorCommandProperty, value);
        }

        private void ErrorsGridRowOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var row = (DataGridRow)sender;
                var errorItem = (ErrorItem)row.Item;
                NavigateToErrorCommand.Execute(errorItem);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogManager.ForContext<ErrorsGrid>().Error(ex, "Navigate to error item exception.");
            }
        }

        // Send scrolling to the parent DataGrid.
        private void ErrorsGridOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_errorsGridScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                return;
            }

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
