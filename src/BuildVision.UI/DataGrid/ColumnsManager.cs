using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BuildVision.Contracts;
using BuildVision.UI.Modelss;
using BuildVision.UI.Helpers;
using BuildVision.UI.Extensions;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models.Columns;
using BuildVision.UI.Settings.Models.Sorting;
using BuildVision.UI.Settings.Models;

namespace BuildVision.UI.DataGrid
{
    public static class ColumnsManager
    {
        private static List<string> _nonSortableColumns = new List<string>
        {
            nameof(ProjectItem.StateBitmap)
        };

        private static List<string> _nonGroupableColumns = new List<string>
        {
            nameof(ProjectItem.StateBitmap),
            nameof(ProjectItem.BuildStartTime),
            nameof(ProjectItem.BuildFinishTime),
            nameof(ProjectItem.BuildElapsedTime),
            nameof(ProjectItem.BuildOrder),
        };

        private static readonly Type _itemRowType = typeof(ProjectItem);

        public static string GetInitialColumnHeader(GridColumnSettings gridColumnSettings)
        {
            try
            {
                var attr = GetPropertyAttribute<GridColumnAttribute>(gridColumnSettings.PropertyNameId);
                return (attr != null) ? attr.Header : null;
            }
            catch (PropertyNotFoundException)
            {
                return null;
            }
        }

        public static object GetColumnExampleValue(GridColumnSettings gridColumnSettings)
        {
            try
            {
                var attr = GetPropertyAttribute<GridColumnAttribute>(gridColumnSettings.PropertyNameId);
                return attr?.ExampleValue;
            }
            catch (PropertyNotFoundException)
            {
                return null;
            }
        }

        public static bool ColumnIsSortable(string propertyName)
        {
            if (_nonSortableColumns.Contains(propertyName))
                return false;
            return true;
        }

        public static bool ColumnIsGroupable(GridColumnSettings gridColumnSettings)
        {
            if (_nonGroupableColumns.Contains(gridColumnSettings.PropertyNameId))
                return false;
            return true;
        }

        public static void GenerateColumns(ObservableCollection<DataGridColumn> columns, GridSettings gridSettings)
        {
            try
            {
                try
                {
                    columns.Clear();
                }
                catch (ArgumentOutOfRangeException)
                {
                    // DataGrid or DataGridColumn issue?
                    // Saving VisualStudio|Options|BuildVision settings causes ArgumentOutOfRangeException (DisplayIndex) 
                    // at columns.Clear if BuildVision toolwindow was hidden since VisualStudio start (only in this case).
                    // For the second time without any errors.
                    columns.Clear();
                }

                var tmpColumnsList = new List<DataGridColumn>();
                foreach (PropertyInfo property in GetItemProperties())
                {
                    GridColumnAttribute columnConfiguration = property.GetCustomAttribute<GridColumnAttribute>();
                    if (columnConfiguration == null)
                        continue;

                    string propertyName = property.Name;
                    GridColumnSettings columnSettings;

                    if (gridSettings.Columns[propertyName] != null)
                    {
                        columnSettings = gridSettings.Columns[propertyName];
                    }
                    else
                    {
                        columnSettings = new GridColumnSettings(
                            propertyName, 
                            columnConfiguration.Header, 
                            columnConfiguration.Visible, 
                            columnConfiguration.DisplayIndex, 
                            columnConfiguration.Width, 
                            columnConfiguration.ValueStringFormat);
                        gridSettings.Columns.Add(columnSettings);
                    }

                    DataGridBoundColumn column = CreateColumn(property);
                    InitColumn(column, columnConfiguration, columnSettings, gridSettings.Sort);
                    tmpColumnsList.Add(column);
                }

                tmpColumnsList.Sort((col1, col2) => col1.DisplayIndex.CompareTo(col2.DisplayIndex));
                for (int i = 0; i < tmpColumnsList.Count; i++)
                {
                    var column = tmpColumnsList[i];

                    // We aren't able to afford coincidence of indexes, otherwise UI will hang.
                    column.DisplayIndex = i;
                    columns.Add(column);
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public static void SyncColumnSettings(ObservableCollection<DataGridColumn> columns, GridSettings gridSettings)
        {
            try
            {
                foreach (DataGridBoundColumn column in columns.OfType<DataGridBoundColumn>())
                {
                    string propertyName = column.GetBindedProperty();
                    GridColumnSettings columnSettings = gridSettings.Columns[propertyName];
                    if (columnSettings == null)
                        continue;

                    columnSettings.Visible = (column.Visibility == Visibility.Visible);
                    columnSettings.DisplayIndex = column.DisplayIndex;
                    columnSettings.Width = column.Width.IsAuto ? double.NaN : column.ActualWidth;
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public static string GetBindedProperty(this DataGridColumn column)
        {
            var boundColumn = column as DataGridBoundColumn;
            if (boundColumn == null)
                return string.Empty;

            var binding = boundColumn.Binding as Binding;
            if (binding == null)
                return string.Empty;

            return binding.Path.Path;
        }

        private static PropertyInfo[] GetItemProperties()
        {
            return _itemRowType.GetProperties();
        }

        private static T GetPropertyAttribute<T>(string propertyName)
            where T : Attribute
        {
            var propertyInfo = _itemRowType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                var ex = new PropertyNotFoundException(propertyName, _itemRowType);
                ex.Trace("Unable to find attribute by property.");
                throw ex;
            }

            T attribute = propertyInfo.GetCustomAttribute<T>();
            return attribute;
        }

        private static DataGridBoundColumn CreateColumn(PropertyInfo property)
        {
            DataGridBoundColumn column;
            column = CreateColumnForProperty(property);
            column.CanUserSort = ColumnIsSortable(property.Name);
            column.Binding = new Binding(property.Name);
            return column;
        }

        private static DataGridBoundColumn CreateColumnForProperty(PropertyInfo property)
        {
            DataGridBoundColumn column;
            if (property.PropertyType == typeof(BitmapSource) || property.PropertyType == typeof(ImageSource))
                column = new DataGridImageColumn();
            else if (property.PropertyType == typeof(ControlTemplate))
                column = new DataGridContentControlColumn();
            else if (property.PropertyType == typeof(bool))
                column = new DataGridCheckBoxColumn();
            else
                column = new DataGridTextColumn();
            return column;
        }

        private static void InitColumn(DataGridBoundColumn column, GridColumnAttribute columnConfiguration, GridColumnSettings columnSettings, SortDescription sortDescription)
        {
            if (string.IsNullOrEmpty(columnConfiguration.ImageKey))
            {
                column.Header = columnSettings.Header;
            }
            else if (columnConfiguration.ImageKey == GridColumnAttribute.EmptyHeaderImageKey)
            {
                column.Header = null;
            }
            else
            {
                const int ImgHeight = 14;
                const int ImgWidth = 14;

                if (string.IsNullOrEmpty(columnConfiguration.ImageDictionaryUri))
                {
                    var imgRes = Resources.ResourceManager.GetObject(columnConfiguration.ImageKey);
                    var img = (System.Drawing.Bitmap)imgRes;
                    column.Header = new Image
                    {
                        Source = img.ToMediaBitmap(),
                        Width = ImgWidth,
                        Height = ImgHeight,
                        Stretch = Stretch.Uniform,
                        Tag = columnSettings.Header
                    };
                }
                else
                {
                    var controlTemplate = VectorResources.TryGet(columnConfiguration.ImageDictionaryUri, columnConfiguration.ImageKey);
                    column.Header = new ContentControl
                    {
                        Template = controlTemplate,
                        Width = ImgWidth,
                        Height = ImgHeight,
                        ClipToBounds = true,
                        Tag = columnSettings.Header
                    };
                }
            }

            column.Visibility = columnSettings.Visible ? Visibility.Visible : Visibility.Collapsed;

            if (columnSettings.DisplayIndex != -1)
                column.DisplayIndex = columnSettings.DisplayIndex;

            if (!double.IsNaN(columnSettings.Width))
                column.Width = new DataGridLength(columnSettings.Width);

            if (columnSettings.ValueStringFormat != null)
                column.Binding.StringFormat = columnSettings.ValueStringFormat;

            if (column.GetBindedProperty() == sortDescription.Property)
                column.SortDirection = sortDescription.Order.ToSystem();

            string columnName = columnSettings.Header;
            if (string.IsNullOrEmpty(columnName))
                columnName = columnConfiguration.Header;

            column.SetValue(DataGridColumnExtensions.NameProperty, columnName);
        }
    }
}