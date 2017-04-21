using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AlekseyNagovitsyn.BuildVision.Tool.Views.Extensions;

using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;
using UIElement = System.Windows.UIElement;
using BuildVision.Contracts;
using AlekseyNagovitsyn.BuildVision.Core.Logging;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views
{
    public partial class ProjectItemDetailsView : UserControl
    {
        private ScrollViewer _errorsGridScrollViewer;

        public static readonly DependencyProperty ProjectItemProperty = 
            DependencyProperty.Register("ProjectItem", typeof(ProjectItem), typeof(ProjectItemDetailsView));

        public ProjectItem ProjectItem
        {
            get { return (ProjectItem)GetValue(ProjectItemProperty); }
            set { SetValue(ProjectItemProperty, value); }
        }

        public ProjectItemDetailsView()
        {
            InitializeComponent();
        }

        private void ProjectItemDetailsViewOnLoaded(object sender, RoutedEventArgs e)
        {
            _errorsGridScrollViewer = VisualHelper.FindVisualChild<ScrollViewer>(ErrorsGrid);
            Debug.Assert(_errorsGridScrollViewer != null);
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
    }
}
