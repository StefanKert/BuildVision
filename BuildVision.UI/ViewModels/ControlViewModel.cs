using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;

using Process = System.Diagnostics.Process;
using Microsoft.VisualStudio;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.DataGrid;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Settings.Models.Columns;
using SortDescription = BuildVision.UI.Settings.Models.Sorting.SortDescription;
using BuildVision.UI.Settings.Models;
using BuildVision.Helpers;

namespace BuildVision.UI.ViewModels
{
    public class ControlViewModel : BindableBase
    {
        private BuildState _buildState;
        private IBuildInfo _buildInfo;
        private ObservableCollection<DataGridColumn> _gridColumnsRef;

        public bool HideUpToDateTargets
        {
            get => ControlSettings.GeneralSettings.HideUpToDateTargets;
            set => SetProperty(() => ControlSettings.GeneralSettings.HideUpToDateTargets, val => ControlSettings.GeneralSettings.HideUpToDateTargets = val, value);
        }

        public ControlModel Model { get; }

        public BuildProgressViewModel BuildProgressViewModel { get; }

        public ControlSettings ControlSettings { get; }

        public ControlTemplate ImageCurrentState
        {
            get => Model.ImageCurrentState;
            set => SetProperty(() => Model.ImageCurrentState, val => Model.ImageCurrentState = val, value);
        }

        public ControlTemplate ImageCurrentStateResult
        {
            get => Model.ImageCurrentStateResult;
            set => SetProperty(() => Model.ImageCurrentStateResult, val => Model.ImageCurrentStateResult = val, value);
        }

        public string TextCurrentState
        {
            get => Model.TextCurrentState;
            set => SetProperty(() => Model.TextCurrentState, val => Model.TextCurrentState = val, value);
        }

        public ProjectItem CurrentProject
        {
            get => Model.CurrentProject;
            set => SetProperty(() => Model.CurrentProject, val => Model.CurrentProject = val, value);
        }

        public ObservableCollection<ValueIndicator> ValueIndicators => Model.ValueIndicators;

        public SolutionItem SolutionItem => Model.SolutionItem;

        public ObservableCollection<ProjectItem> ProjectsList => Model.SolutionItem.Projects;

        public string GridGroupPropertyName
        {
            get { return ControlSettings.GridSettings.GroupName; }
            set
            {
                if (ControlSettings.GridSettings.GroupName != value)
                {
                    ControlSettings.GridSettings.GroupName = value;
                    OnPropertyChanged(nameof(GridGroupPropertyName));
                    OnPropertyChanged(nameof(GroupedProjectsList));
                    OnPropertyChanged(nameof(GridColumnsGroupMenuItems));
                    OnPropertyChanged(nameof(GridGroupHeaderName));
                }
            }
        }

        public string GridGroupHeaderName
        {
            get
            {
                if (string.IsNullOrEmpty(GridGroupPropertyName))
                    return string.Empty;

                return ControlSettings.GridSettings.Columns[GridGroupPropertyName].Header;
            }
        }

        public CompositeCollection GridColumnsGroupMenuItems => CreateContextMenu();

        private CompositeCollection CreateContextMenu()
        {
            var collection = new CompositeCollection();
            collection.Add(new MenuItem
            {
                Header = Resources.NoneMenuItem,
                Tag = string.Empty
            });

            foreach (GridColumnSettings column in ControlSettings.GridSettings.Columns)
            {
                if (!ColumnsManager.ColumnIsGroupable(column))
                    continue;

                string header = column.Header;
                var menuItem = new MenuItem
                {
                    Header = !string.IsNullOrEmpty(header)
                                ? header
                                : ColumnsManager.GetInitialColumnHeader(column),
                    Tag = column.PropertyNameId
                };

                collection.Add(menuItem);
            }

            foreach (MenuItem menuItem in collection)
            {
                menuItem.IsCheckable = false;
                menuItem.StaysOpenOnClick = false;
                menuItem.IsChecked = (GridGroupPropertyName == (string) menuItem.Tag);
                menuItem.Command = GridGroupPropertyMenuItemClicked;
                menuItem.CommandParameter = menuItem.Tag;
            }

            return collection;
        }

        public SortDescription GridSortDescription
        {
            get => ControlSettings.GridSettings.Sort;
            set
            {
                if (ControlSettings.GridSettings.Sort != value)
                {
                    ControlSettings.GridSettings.Sort = value;
                    OnPropertyChanged(nameof(GridSortDescription));
                    OnPropertyChanged(nameof(GroupedProjectsList));
                }
            }
        }

        // Should be initialized by View.
        public void SetGridColumnsRef(ObservableCollection<DataGridColumn> gridColumnsRef)
        {
            if (_gridColumnsRef != gridColumnsRef)
            {
                _gridColumnsRef = gridColumnsRef;
                GenerateColumns();
            }
        }

        // TODO: Rewrite using CollectionViewSource? 
        // http://stackoverflow.com/questions/11505283/re-sort-wpf-datagrid-after-bounded-data-has-changed
        public ListCollectionView GroupedProjectsList
        {
            get
            {
                var groupedList = new ListCollectionView(ProjectsList);

                if (!string.IsNullOrWhiteSpace(GridGroupPropertyName))
                {
                    Debug.Assert(groupedList.GroupDescriptions != null);
                    groupedList.GroupDescriptions.Add(new PropertyGroupDescription(GridGroupPropertyName));
                }

                groupedList.CustomSort = GetProjectItemSorter(GridSortDescription);
                groupedList.IsLiveGrouping  = true;
                groupedList.IsLiveSorting = true;
                return groupedList;
            }
        }

        public DataGridHeadersVisibility GridHeadersVisibility
        {
            get
            {
                return ControlSettings.GridSettings.ShowColumnsHeader
                    ? DataGridHeadersVisibility.Column
                    : DataGridHeadersVisibility.None;
            }
            set
            {
                bool showColumnsHeader = (value != DataGridHeadersVisibility.None);
                if (ControlSettings.GridSettings.ShowColumnsHeader != showColumnsHeader)
                {
                    ControlSettings.GridSettings.ShowColumnsHeader = showColumnsHeader;
                    OnPropertyChanged(nameof(GridHeadersVisibility));
                }
            }
        }

        private ProjectItem _selectedProjectItem;
        public ProjectItem SelectedProjectItem
        {
            get => _selectedProjectItem; 
            set => SetProperty(ref _selectedProjectItem, value);
        }

        public ControlViewModel(ControlModel model, ControlSettings controlSettings)
        {
            Model = model;
            ControlSettings = controlSettings;
            BuildProgressViewModel = new BuildProgressViewModel(ControlSettings);
        }

        /// <summary>
        /// Uses as design-time ViewModel. 
        /// </summary>
        internal ControlViewModel()
        {
            Model = new ControlModel();
            ControlSettings = new ControlSettings();
            BuildProgressViewModel = new BuildProgressViewModel(ControlSettings);
        }

        private void OpenContainingFolder()
        {
            try
            {
                string dir = Path.GetDirectoryName(SelectedProjectItem.FullName);
                Debug.Assert(dir != null);
                Process.Start(dir);
            }
            catch (Exception ex)
            {                
                ex.Trace(string.Format(
                    "Unable to open folder '{0}' containing the project '{1}'.",
                    SelectedProjectItem.FullName,
                    SelectedProjectItem.UniqueName));

                MessageBox.Show(
                    ex.Message + "\n\nSee log for details.",
                    Resources.ProductName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ReorderGrid(object obj)
        {
            var e = (DataGridSortingEventArgs)obj;

            ListSortDirection? oldSortDirection = e.Column.SortDirection;
            ListSortDirection? newSortDirection;
            switch (oldSortDirection)
            {
                case null:
                    newSortDirection = ListSortDirection.Ascending;
                    break;
                case ListSortDirection.Ascending:
                    newSortDirection = ListSortDirection.Descending;
                    break;
                case ListSortDirection.Descending:
                    newSortDirection = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj));
            }

            e.Handled = true;
            e.Column.SortDirection = newSortDirection;

            GridSortDescription = new SortDescription(newSortDirection.ToMedia(), e.Column.GetBindedProperty());
        }

        private static ProjectItemColumnSorter GetProjectItemSorter(SortDescription sortDescription)
        {
            SortOrder sortOrder = sortDescription.Order;
            string sortPropertyName = sortDescription.Property;

            if (sortOrder != SortOrder.None && !string.IsNullOrEmpty(sortPropertyName))
            {
                ListSortDirection? sortDirection = sortOrder.ToSystem();
                Debug.Assert(sortDirection != null);

                try
                {
                    return new ProjectItemColumnSorter(sortDirection.Value, sortPropertyName);
                }
                catch (PropertyNotFoundException ex)
                {
                    ex.Trace("Trying to sort Project Items by nonexistent property.");
                    return null;
                }
            }

            return null;
        }

        public void ResetIndicators(ResetIndicatorMode resetMode)
        {
            foreach (ValueIndicator indicator in ValueIndicators)
                indicator.ResetValue(resetMode);

            OnPropertyChanged(nameof(ValueIndicators));
        }

        public void UpdateIndicators(IBuildInfo buildContext)
        {
            foreach (ValueIndicator indicator in ValueIndicators)
                indicator.UpdateValue(buildContext);

            OnPropertyChanged(nameof(ValueIndicators));
        }

        public void GenerateColumns()
        {
            Debug.Assert(_gridColumnsRef != null);
            ColumnsManager.GenerateColumns(_gridColumnsRef, ControlSettings.GridSettings);
        }

        public void SyncColumnSettings()
        {
            Debug.Assert(_gridColumnsRef != null);
            ColumnsManager.SyncColumnSettings(_gridColumnsRef, ControlSettings.GridSettings);
        }

        public void OnControlSettingsChanged(ControlSettings settings, Func<IBuildInfo, string> getBuildMessage)
        {
            ControlSettings.InitFrom(settings);

            GenerateColumns();

            if (_buildState == BuildState.Done)
            {
                Model.TextCurrentState = getBuildMessage(_buildInfo);
            }

            // Raise all properties have changed.
            OnPropertyChanged(null);

            BuildProgressViewModel.ResetTaskBarInfo(false);
        }

        public void OnBuildProjectBegin()
        {
            BuildProgressViewModel.OnBuildProjectBegin();
        }

        public void OnBuildProjectDone(BuildedProject buildedProjectInfo)
        {
            bool success = buildedProjectInfo.Success.GetValueOrDefault(true);
            BuildProgressViewModel.OnBuildProjectDone(success);
        }

        public void OnBuildBegin(int projectsCount, IBuildInfo buildContext)
        {
            _buildState = BuildState.InProgress;
            _buildInfo = buildContext;
            BuildProgressViewModel.OnBuildBegin(projectsCount);
        }

        public void OnBuildDone(IBuildInfo buildInfo)
        {
            _buildInfo = buildInfo;
            _buildState = BuildState.Done;
            BuildProgressViewModel.OnBuildDone();
        }

        public void OnBuildCancelled(IBuildInfo buildInfo)
        {
            _buildInfo = buildInfo;
            BuildProgressViewModel.OnBuildCancelled();
        }

        private bool IsProjectItemEnabledForActions()
        {
            return (SelectedProjectItem != null && !string.IsNullOrEmpty(SelectedProjectItem.UniqueName) && !SelectedProjectItem.IsBatchBuildProject);
        }

        #region Commands

        public ICommand ReportIssues => new RelayCommand(obj => GithubHelper.OpenBrowserWithPrefilledIssue());

        public ICommand GridSorting => new RelayCommand(obj => ReorderGrid(obj));

        public ICommand GridGroupPropertyMenuItemClicked => new RelayCommand(obj => GridGroupPropertyName = (obj != null) ? obj.ToString() : string.Empty);

        public ICommand SelectedProjectOpenContainingFolderAction => new RelayCommand(obj => OpenContainingFolder(),
                canExecute: obj => (SelectedProjectItem != null && !string.IsNullOrEmpty(SelectedProjectItem.FullName)));
    
        public ICommand SelectedProjectCopyBuildOutputFilesToClipboardAction => new RelayCommand(
            obj => ProjectCopyBuildOutputFilesToClipBoard(SelectedProjectItem),
            canExecute: obj => (SelectedProjectItem != null && !string.IsNullOrEmpty(SelectedProjectItem.UniqueName) && !ControlSettings.ProjectItemSettings.CopyBuildOutputFileTypesToClipboard.IsEmpty));

        public ICommand SelectedProjectBuildAction => new RelayCommand(
            obj => RaiseCommandForSelectedProject(SelectedProjectItem, (int)VSConstants.VSStd97CmdID.BuildCtx),
            canExecute: obj => IsProjectItemEnabledForActions());


        public ICommand SelectedProjectRebuildAction => new RelayCommand(
            obj => RaiseCommandForSelectedProject(SelectedProjectItem, (int)VSConstants.VSStd97CmdID.RebuildCtx),
            canExecute: obj => IsProjectItemEnabledForActions());

        public ICommand SelectedProjectCleanAction => new RelayCommand(
            obj => RaiseCommandForSelectedProject(SelectedProjectItem, (int)VSConstants.VSStd97CmdID.CleanCtx),
            canExecute: obj => IsProjectItemEnabledForActions());

        public ICommand SelectedProjectCopyErrorMessagesAction => new RelayCommand(obj => CopyErrorMessageToClipboard(SelectedProjectItem),
        canExecute: obj => SelectedProjectItem?.ErrorsCount > 0);

        public ICommand BuildSolutionAction => new RelayCommand(obj => BuildSolution());

        public ICommand RebuildSolutionAction => new RelayCommand(obj => RebuildSolution());

        public ICommand CleanSolutionAction => new RelayCommand(obj => CleanSolution());

        public ICommand CancelBuildSolutionAction => new RelayCommand(obj => CancelBuildSolution());

        public ICommand OpenGridColumnsSettingsAction => new RelayCommand(obj => ShowGridColumnsSettingsPage()); 

        public ICommand OpenGeneralSettingsAction => new RelayCommand(obj => ShowGeneralSettingsPage()); 

        #endregion

        public event Action ShowGridColumnsSettingsPage;
        public event Action ShowGeneralSettingsPage;
        public event Action BuildSolution;
        public event Action CleanSolution;
        public event Action RebuildSolution;
        public event Action CancelBuildSolution;
        public event Action<ProjectItem> ProjectCopyBuildOutputFilesToClipBoard;
        public event Action<ProjectItem, int> RaiseCommandForSelectedProject;
        public event Action<ProjectItem> CopyErrorMessageToClipboard;
    }
}
