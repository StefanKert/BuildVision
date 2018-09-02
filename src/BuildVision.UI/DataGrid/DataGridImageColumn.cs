using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BuildVision.UI.DataGrid
{
    public class DataGridImageColumn : DataGridBoundColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            Image image = (cell != null) ? (cell.Content as Image) : null;
            if (image == null)
                image = new Image();

            image.Stretch = Stretch.None;
            image.SnapsToDevicePixels = true;
            BindingOperations.SetBinding(image, Image.SourceProperty, Binding);
            return image;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            throw new InvalidOperationException();
        }
    }
}