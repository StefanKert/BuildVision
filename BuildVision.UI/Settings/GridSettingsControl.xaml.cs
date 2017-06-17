using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Microsoft.VisualStudio.Shell;
using BuildVision.UI.Settings.Models.Columns;
using BuildVision.UI.Extensions;

namespace BuildVision.UI.Settings
{
    public partial class GridSettingsControl : UserControl
    {
        private int _oldDisplayIndex;
        private int _newDisplayIndex;
        private bool _displayIndexCommiting;

        public GridSettingsControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Grid.AddHandler(UIElementDialogPage.DialogKeyPendingEvent, new RoutedEventHandler(OnDialogKeyPendingEvent));
            Grid.TargetUpdated += GridOnTargetUpdated;
            Grid.CellEditEnding += GridOnCellEditEnding;
            Grid.CurrentCellChanged += GridOnCurrentCellChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // http://stackoverflow.com/questions/15025865/wpf-datagrid-not-exiting-edit-mode
            if (e.NewValue == null)
                Grid.CommitEdit();
        }

        private void OnDialogKeyPendingEvent(object sender, RoutedEventArgs e)
        {
            // Prevent exit from DialogPage.
            var args = e as DialogKeyEventArgs;
            if (args != null && (args.Key == Key.Enter || args.Key == Key.Escape))
            {
                e.Handled = true;
            }
        }

        private void GridOnCurrentCellChanged(object sender, EventArgs e)
        {
            if (Grid.CurrentCell.IsValid)
            {
                var currentItem = (GridColumnSettings)Grid.CurrentCell.Item;
                _oldDisplayIndex = currentItem.DisplayIndex;
            }
        }

        private void GridOnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (_displayIndexCommiting)
                return;

            if (!ReferenceEquals(e.Column, ColumnDisplayIndex))
                return;

            if (e.EditAction != DataGridEditAction.Commit)
                return;

            _displayIndexCommiting = true;
            bool commited = Grid.CommitEdit(DataGridEditingUnit.Row, true);
            _displayIndexCommiting = false;

            if (!commited)
                return;

            var editedItem = (GridColumnSettings)e.Row.Item;
            _newDisplayIndex = editedItem.DisplayIndex;

            if (_newDisplayIndex < 0)
            {
                _newDisplayIndex = 0;
                editedItem.DisplayIndex = _newDisplayIndex;
            }
            else if (_newDisplayIndex >= Grid.Items.Count)
            {
                _newDisplayIndex = Grid.Items.Count - 1;
                editedItem.DisplayIndex = _newDisplayIndex;
            }

            if (_oldDisplayIndex < _newDisplayIndex)
            {
                foreach (GridColumnSettings item in Grid.Items)
                {
                    if (ReferenceEquals(item, editedItem))
                        continue;

                    if (item.DisplayIndex >= _oldDisplayIndex + 1 && item.DisplayIndex <= _newDisplayIndex)
                        item.DisplayIndex -= 1;
                }
            }
            else
            {
                foreach (GridColumnSettings item in Grid.Items)
                {
                    if (ReferenceEquals(item, editedItem))
                        continue;

                    if (item.DisplayIndex >= _newDisplayIndex && item.DisplayIndex <= _oldDisplayIndex - 1)
                        item.DisplayIndex += 1;
                }
            }

            Grid.UpdateTarget(ItemsControl.ItemsSourceProperty);
        }

        private void GridOnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property.Name == "ItemsSource")
            {
                var grid = (System.Windows.Controls.DataGrid)sender;
                if (grid.ItemsSource == null)
                    return;

                grid.Items.SortDescriptions.Add(new SortDescription("DisplayIndex", ListSortDirection.Ascending));

                ColumnDisplayIndex.SortDirection = ListSortDirection.Ascending;
                ColumnDisplayIndex.SortMemberPath = "DisplayIndex";
            }
        }
    }
}