using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Tool.Views.Extensions;
using BuildVision.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
