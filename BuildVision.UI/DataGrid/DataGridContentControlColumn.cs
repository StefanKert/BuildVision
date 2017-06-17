using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BuildVision.UI.DataGrid
{
    public class DataGridContentControlColumn : DataGridBoundColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            ContentControl contentControl = (cell != null) ? (cell.Content as ContentControl) : null;
            if (contentControl == null)
                contentControl = new ContentControl();

            contentControl.ClipToBounds = true;
            contentControl.SnapsToDevicePixels = true;
            contentControl.Width = 16;
            contentControl.Height = 16;
            BindingOperations.SetBinding(contentControl, Control.TemplateProperty, Binding);
            return contentControl;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            throw new InvalidOperationException();
        }
    }
}