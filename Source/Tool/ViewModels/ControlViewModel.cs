using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Microsoft.VisualStudio;
using EnvDTE;

using AlekseyNagovitsyn.BuildVision.Core.Common;
using AlekseyNagovitsyn.BuildVision.Tool.Building;
using AlekseyNagovitsyn.BuildVision.Core.Logging;
using AlekseyNagovitsyn.BuildVision.Helpers;
using AlekseyNagovitsyn.BuildVision.Tool.DataGrid;
using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Indicators.Core;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting;
using AlekseyNagovitsyn.BuildVision.Tool.Views.Settings;

using Process = System.Diagnostics.Process;
using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;
using SortDescription = AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Sorting.SortDescription;

namespace AlekseyNagovitsyn.BuildVision.Tool.ViewModels
{
    public class ControlViewModel : ViewModelBase
    {
        private readonly ControlModel _model;
        private readonly ControlSettings _settings;
        private readonly BuildProgressViewModel _buildProgressViewModel;
        private readonly IPackageContext _packageContext;

        private BuildState _buildState;
        private BuildInfo _buildInfo;
        private ObservableCollection<DataGridColumn> _gridColumnsRef;

        public ControlViewModel(ControlModel model, IPackageContext packageContext)
        {
            _model = model;
            _packageContext = packageContext;

            _settings = packageContext.ControlSettings;
            _buildProgressViewModel = new BuildProgressViewModel(_settings);

            packageContext.ControlSettingsChanged += OnControlSettingsChanged;
        }

        /// <summary>
        /// Uses as design-time ViewModel. 
        /// </summary>
        internal ControlViewModel()
        {
            _model = new ControlModel();
            _settings = new ControlSettings();
            _buildProgressViewModel = new BuildProgressViewModel(_settings);
        }

        public ControlModel Model
        {
            get { return _model; }
        }

        public BuildProgressViewModel BuildProgressViewModel
        {
            get { return _buildProgressViewModel; }
        }

        public ControlSettings ControlSettings
        {
            get { return _settings; }
        }

        public ControlTemplate ImageCurrentState
        {
            get 
            {
                return Model.ImageCurrentState;
            }
            set
            {
                if (Model.ImageCurrentState != value)
                {
                    Model.ImageCurrentState = value;
                    OnPropertyChanged("ImageCurrentState");
                }
            }
        }

        public ControlTemplate ImageCurrentStateResult
        {
            get
            {
                return Model.ImageCurrentStateResult;
            }
            set
            {
                if (Model.ImageCurrentStateResult != value)
                {
                    Model.ImageCurrentStateResult = value;
                    OnPropertyChanged("ImageCurrentStateResult");
                }
            }
        }

        public string TextCurrentState
        {
            get { return Model.TextCurrentState; }
            set
            {
                if (Model.TextCurrentState != value)
                {
                    Model.TextCurrentState = value;
                    OnPropertyChanged("TextCurrentState");
                }
            }
        }

        public ProjectItem CurrentProject
        {
            get { return Model.CurrentProject; }
            set
            {
                if (Model.CurrentProject != value)
                {
                    Model.CurrentProject = value;
                    OnPropertyChanged("CurrentProject");
                }
            }
        }

        public ObservableCollection<ValueIndicator> ValueIndicators
        {
            get { return Model.ValueIndicators; }
        }

        public void ResetIndicators(ResetIndicatorMode resetMode)
        {
            foreach (ValueIndicator indicator in ValueIndicators)
                indicator.ResetValue(resetMode);

            OnPropertyChanged("ValueIndicators");
        }

        public void UpdateIndicators(DTE dte, BuildInfo buildContext)
        {
            foreach (ValueIndicator indicator in ValueIndicators)
                indicator.UpdateValue(dte, buildContext);

            OnPropertyChanged("ValueIndicators");
        }

        public SolutionItem SolutionItem
        {
            get { return Model.SolutionItem; }
        }

        public ObservableCollection<ProjectItem> ProjectsList
        {
            get { return Model.SolutionItem.Projects; }
        }

        #region Grid grouping

        public string GridGroupPropertyName
        {
            get { return _settings.GridSettings.GroupPropertyName; }
            set
            {
                if (_settings.GridSettings.GroupPropertyName != value)
                {
                    _settings.GridSettings.GroupPropertyName = value;
                    OnPropertyChanged("GridGroupPropertyName");
                    OnPropertyChanged("GroupedProjectsList");
                    OnPropertyChanged("GridColumnsGroupMenuItems");
                    OnPropertyChanged("GridGroupHeaderName");
                }
            }
        }

        public string GridGroupHeaderName
        {
            get
            {
                if (string.IsNullOrEmpty(GridGroupPropertyName))
                    return string.Empty;

                return _settings.GridSettings.Columns[GridGroupPropertyName].Header;
            }
        }

        public CompositeCollection GridColumnsGroupMenuItems
        {
            get
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
                    menuItem.IsChecked = (GridGroupPropertyName == (string)menuItem.Tag);
                    menuItem.Command = GridGroupPropertyMenuItemClicked;
                    menuItem.CommandParameter = menuItem.Tag;
                }

                return collection;
            }
        }

        public ICommand GridGroupPropertyMenuItemClicked
        {
            get
            {
                return new RelayCommand(obj => 
                {
                    GridGroupPropertyName = (obj != null) ? obj.ToString() : string.Empty;
                });
            }
        }

        #endregion //Grid grouping

        #region Grid sorting

        public SortDescription GridSortDescription
        {
            get { return _settings.GridSettings.SortDescription; }
            set
            {
                if (_settings.GridSettings.SortDescription != value)
                {
                    _settings.GridSettings.SortDescription = value;
                    OnPropertyChanged("GridSortDescription");
                    OnPropertyChanged("GroupedProjectsList");
                }
            }
        }

        public ICommand GridSorting
        {
            get
            {
                return new RelayCommand(obj =>
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
                            throw new ArgumentOutOfRangeException();
                    }

                    e.Handled = true;
                    e.Column.SortDirection = newSortDirection;

                    GridSortDescription = new SortDescription(newSortDirection.ToMedia(), e.Column.GetBindedProperty());
                });
            }
        }

        #endregion //Grid sorting

        public ICommand OpenGridColumnsSettingsAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _packageContext.ShowOptionPage(typeof(GridSettingsDialogPage));
                });
            }
        }

        // Should be initialized by View.
        public ObservableCollection<DataGridColumn> GridColumnsRef
        {
            set
            {
                if (_gridColumnsRef != value)
                {
                    _gridColumnsRef = value;
                    GenerateColumns();
                }
            }
        }

        public void GenerateColumns()
        {
            Debug.Assert(_gridColumnsRef != null);
            ColumnsManager.GenerateColumns(_gridColumnsRef, _packageContext.ControlSettings.GridSettings);
        }

        public void SyncColumnSettings()
        {
            Debug.Assert(_gridColumnsRef != null);
            ColumnsManager.SyncColumnSettings(_gridColumnsRef, _packageContext.ControlSettings.GridSettings);
        }

        private void OnControlSettingsChanged(object sender, EventArgs eventArgs)
        {
            var package = (IPackageContext)sender;
            _settings.InitFrom(package.ControlSettings);

            GenerateColumns();

            if (_buildState == BuildState.Done)
            {
                _model.TextCurrentState = BuildMessages.GetBuildDoneMessage(
                    Model.SolutionItem,
                    _buildInfo,
                    _settings.BuildMessagesSettings);
            }

            // Raise all properties have changed.
            OnPropertyChanged(null);

            _buildProgressViewModel.ResetTaskBarInfo(false);
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

                return groupedList;
            }
        }

        private static ProjectItemColumnSorter GetProjectItemSorter(SortDescription sortDescription)
        {
            SortOrder sortOrder = sortDescription.SortOrder;
            string sortPropertyName = sortDescription.SortPropertyName;

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

        public void OnBuildProjectBegin()
        {
            BuildProgressViewModel.OnBuildProjectBegin();
        }

        public void OnBuildProjectDone(BuildedProject buildedProjectInfo)
        {
            bool success = buildedProjectInfo.Success.GetValueOrDefault(true);
            BuildProgressViewModel.OnBuildProjectDone(success);
        }

        public void OnBuildBegin(BuildInfo buildContext)
        {
            _buildState = BuildState.InProgress;
            _buildInfo = buildContext;

            int projectsCount = -1;
            switch (buildContext.BuildScope)
            {
                case vsBuildScope.vsBuildScopeSolution:
                    if (ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                    {
                        projectsCount = ProjectsList.Count;
                    }
                    else
                    {
                        try
                        {
                            Solution solution = SolutionItem.StorageSolution;
                            if (solution != null)
                                projectsCount = solution.GetProjects().Count;
                        }
                        catch (Exception ex)
                        {
                            ex.Trace("Unable to count projects in solution.");
                        }
                    }
                    break;

                case vsBuildScope.vsBuildScopeBatch:
                case vsBuildScope.vsBuildScopeProject:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            BuildProgressViewModel.OnBuildBegin(projectsCount);
        }

        public void OnBuildDone(BuildInfo buildInfo)
        {
            _buildInfo = buildInfo;
            _buildState = BuildState.Done;
            BuildProgressViewModel.OnBuildDone();
        }

        public void OnBuildCancelled(BuildInfo buildInfo)
        {
            _buildInfo = buildInfo;
            BuildProgressViewModel.OnBuildCancelled();
        }

        public ProjectItem FindProjectItem(object property, FindProjectProperty findProjectProperty, bool createIfNotFound = true)
        {
            ProjectItem found;
            switch (findProjectProperty)
            {
                case FindProjectProperty.UniqueName:
                    var uniqueName = (string)property;
                    found = ProjectsList.FirstOrDefault(item => item.StorageProject != null && item.StorageProject.UniqueName == uniqueName)
                        ?? ProjectsList.FirstOrDefault(item => item.UniqueName == uniqueName);
                    break;

                case FindProjectProperty.FullName:
                    var fullName = (string)property;
                    found = ProjectsList.FirstOrDefault(item => item.StorageProject != null && item.StorageProject.FullName == fullName)
                        ?? ProjectsList.FirstOrDefault(item => item.FullName == fullName);
                    break;

                case FindProjectProperty.ProjectObject:
                    found = ProjectsList.FirstOrDefault(item => ReferenceEquals(item.StorageProject, property));
                    break;

                case FindProjectProperty.UniqueNameProjectDefinition:
                    {
                        var projDef = (UniqueNameProjectDefinition)property;
                        found = ProjectsList.FirstOrDefault(item => item.StorageProject != null
                                                                && item.StorageProject.UniqueName == projDef.UniqueName
                                                                && item.StorageProject.ConfigurationManager.ActiveConfiguration.ConfigurationName == projDef.Configuration
                                                                && PlatformsIsEquals(item.StorageProject.ConfigurationManager.ActiveConfiguration.PlatformName, projDef.Platform))
                            ?? ProjectsList.FirstOrDefault(item => item.UniqueName == projDef.UniqueName
                                                                && item.Configuration == projDef.Configuration
                                                                && PlatformsIsEquals(item.Platform, projDef.Platform));
                    }
                    break;

                case FindProjectProperty.FullNameProjectDefinition:
                    {
                        var projDef = (FullNameProjectDefinition)property;
                        found = ProjectsList.FirstOrDefault(item => item.StorageProject != null
                                                                && item.StorageProject.FullName == projDef.FullName
                                                                && item.StorageProject.ConfigurationManager.ActiveConfiguration.ConfigurationName == projDef.Configuration
                                                                && PlatformsIsEquals(item.StorageProject.ConfigurationManager.ActiveConfiguration.PlatformName, projDef.Platform))
                            ?? ProjectsList.FirstOrDefault(item => item.FullName == projDef.FullName
                                                                && item.Configuration == projDef.Configuration
                                                                && PlatformsIsEquals(item.Platform, projDef.Platform));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("findProjectProperty");
            }

            if (found != null)
                return found;

            Project proj;
            switch (findProjectProperty)
            {
                case FindProjectProperty.UniqueName:
                    var uniqueName = (string)property;
                    proj = SolutionItem.StorageSolution.GetProject(item => item.UniqueName == uniqueName);
                    break;

                case FindProjectProperty.FullName:
                    var fullName = (string)property;
                    proj = SolutionItem.StorageSolution.GetProject(item => item.FullName == fullName);
                    break;

                case FindProjectProperty.ProjectObject:
                    proj = SolutionItem.StorageSolution.GetProject(item => ReferenceEquals(item, property));
                    break;

                case FindProjectProperty.UniqueNameProjectDefinition:
                    {
                        var projDef = (UniqueNameProjectDefinition)property;
                        proj = SolutionItem.StorageSolution.GetProject(item => item.UniqueName == projDef.UniqueName
                                                                && item.ConfigurationManager.ActiveConfiguration.ConfigurationName == projDef.Configuration
                                                                && PlatformsIsEquals(item.ConfigurationManager.ActiveConfiguration.PlatformName, projDef.Platform));
                    }
                    break;

                case FindProjectProperty.FullNameProjectDefinition:
                    {
                        var projDef = (FullNameProjectDefinition)property;
                        proj = SolutionItem.StorageSolution.GetProject(item => item.FullName == projDef.FullName
                                                                && item.ConfigurationManager.ActiveConfiguration.ConfigurationName == projDef.Configuration
                                                                && PlatformsIsEquals(item.ConfigurationManager.ActiveConfiguration.PlatformName, projDef.Platform));
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException("findProjectProperty");
            }

            if (proj == null)
                return null;

            var newProjItem = new ProjectItem(proj);
            ProjectsList.Add(newProjItem);
            return newProjItem;
        }

        private bool PlatformsIsEquals(string platformName1, string platformName2)
        {
            if (string.Compare(platformName1, platformName2, StringComparison.InvariantCultureIgnoreCase) == 0)
                return true;

            // The ambiguity between Project.ActiveConfiguration.PlatformName and
            // ProjectStartedEventArgs.ProjectPlatform in Microsoft.Build.Utilities.Logger
            // (see BuildOutputLogger).
            bool isAnyCpu1 = (platformName1 == "Any CPU" || platformName1 == "AnyCPU");
            bool isAnyCpu2 = (platformName2 == "Any CPU" || platformName2 == "AnyCPU");
            if (isAnyCpu1 && isAnyCpu2)
                return true;

            return false;
        }

        public DataGridHeadersVisibility GridHeadersVisibility
        {
            get 
            { 
                return _settings.GridSettings.ShowColumnsHeader 
                    ? DataGridHeadersVisibility.Column 
                    : DataGridHeadersVisibility.None; 
            }
            set
            {
                bool showColumnsHeader = (value != DataGridHeadersVisibility.None);
                if (_settings.GridSettings.ShowColumnsHeader != showColumnsHeader)
                {
                    _settings.GridSettings.ShowColumnsHeader = showColumnsHeader;
                    OnPropertyChanged("GridHeadersVisibility");
                }
            }
        }

        private ProjectItem _selectedProjectItem;
        public ProjectItem SelectedProjectItem 
        {
            get { return _selectedProjectItem; }
            set
            {
                if (_selectedProjectItem != value)
                {
                    _selectedProjectItem = value;
                    OnPropertyChanged("SelectedProjectItem");
                }
            }
        }

        public ICommand SelectedProjectOpenContainingFolderAction
        {
            get
            {
                return new RelayCommand(obj =>
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
                },
                canExecute: obj =>
                {
                    return (SelectedProjectItem != null && !string.IsNullOrEmpty(SelectedProjectItem.FullName));
                });
            }
        }

        public ICommand SelectedProjectCopyBuildOutputFilesToClipboardAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        ProjectItem projItem = SelectedProjectItem;
                        Project project = projItem.StorageProject;
                        BuildOutputFileTypes fileTypes = ControlSettings.ProjectItemSettings.CopyBuildOutputFileTypesToClipboard;
                        if (fileTypes.IsEmpty)
                        {
                            MessageBox.Show(
                                @"Nothing to copy: all file types unchecked.", 
                                Resources.ProductName,
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                            return;
                        }

                        string[] filePaths = project.GetBuildOutputFilePaths(fileTypes, projItem.Configuration, projItem.Platform).ToArray();
                        if (filePaths.Length == 0)
                        {
                            MessageBox.Show(
                                @"Nothing copied: selected build output groups are empty.",
                                Resources.ProductName,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            return;
                        }

                        string[] existFilePaths = filePaths.Where(File.Exists).ToArray();
                        if (existFilePaths.Length == 0)
                        {
                            string msg = GetCopyBuildOutputFilesToClipboardActionMessage("Nothing copied. {0} wasn't found{1}", filePaths);
                            MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        ClipboardHelper.SetFileDropList(existFilePaths);

                        if (existFilePaths.Length == filePaths.Length)
                        {
                            string msg = GetCopyBuildOutputFilesToClipboardActionMessage("Copied {0}{1}", existFilePaths);
                            MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            string[] notExistFilePaths = filePaths.Except(existFilePaths).ToArray();
                            string copiedMsg = GetCopyBuildOutputFilesToClipboardActionMessage("Copied {0}{1}", existFilePaths);
                            string notFoundMsg = GetCopyBuildOutputFilesToClipboardActionMessage("{0} wasn't found{1}", notExistFilePaths);
                            string msg = string.Concat(copiedMsg, Environment.NewLine, Environment.NewLine, notFoundMsg);
                            MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        string msg = string.Format(
                            "Error copying files to the Clipboard: 0x{0:X} ({1})", 
                            ex.ErrorCode, 
                            ex.Message);

                        ex.Trace(msg);
                        MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                        MessageBox.Show(ex.Message, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                canExecute: obj =>
                {
                    return (SelectedProjectItem != null 
                        && SelectedProjectItem.StorageProject != null 
                        && !ControlSettings.ProjectItemSettings.CopyBuildOutputFileTypesToClipboard.IsEmpty);
                });
            }
        }

        private string GetCopyBuildOutputFilesToClipboardActionMessage(string template, string[] filePaths)
        {
            const int MaxFilePathLinesInMessage = 30;
            const int MaxFilePathLengthInMessage = 60;

            string filesCountArg = string.Concat(filePaths.Length, " file", filePaths.Length == 1 ? string.Empty : "s");
            string filesListArg;
            if (filePaths.Length < MaxFilePathLinesInMessage)
            {
                IEnumerable<string> shortenedFilePaths = FilePathHelper.ShortenPaths(filePaths, MaxFilePathLengthInMessage);
                filesListArg = string.Concat(":", Environment.NewLine, string.Join(Environment.NewLine, shortenedFilePaths));
            }
            else
            {
                filesListArg = ".";
            }

            string msg = string.Format(template, filesCountArg, filesListArg);
            return msg;
        }

        public ICommand SelectedProjectBuildAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        RaiseCommandForSelectedProject((int)VSConstants.VSStd97CmdID.BuildCtx);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                },
                canExecute: obj =>
                {
                    return (SelectedProjectItem != null && SelectedProjectItem.StorageProject != null
                            && SolutionItem != null && SolutionItem.StorageSolution != null && !SelectedProjectItem.IsBatchBuildProject);
                });
            }
        }

        public ICommand SelectedProjectRebuildAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        RaiseCommandForSelectedProject((int)VSConstants.VSStd97CmdID.RebuildCtx);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                },
                canExecute: obj =>
                {
                    return (SelectedProjectItem != null && SelectedProjectItem.StorageProject != null && !SelectedProjectItem.IsBatchBuildProject);
                });
            }
        }

        public ICommand SelectedProjectCleanAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        RaiseCommandForSelectedProject((int)VSConstants.VSStd97CmdID.CleanCtx);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                },
                canExecute: obj =>
                {
                    return (SelectedProjectItem != null && SelectedProjectItem.StorageProject != null && !SelectedProjectItem.IsBatchBuildProject);
                });
            }
        }

        private void RaiseCommandForSelectedProject(int commandId)
        {
            var dte2 = (EnvDTE80.DTE2)SolutionItem.StorageSolution.DTE;
            UIHierarchy solutionExplorer = dte2.ToolWindows.SolutionExplorer;
            UIHierarchyItem item = solutionExplorer.FindHierarchyItem(SelectedProjectItem.StorageProject);
            if (item == null)
                throw new Exception(string.Format("Project '{0}' not found in SolutionExplorer.", SelectedProjectItem.StorageProject.UniqueName));

            solutionExplorer.Parent.Activate();
            item.Select(vsUISelectionType.vsUISelectionTypeSelect);

            object customIn = null;
            object customOut = null;
            dte2.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), commandId, ref customIn, ref customOut);
        }

        public ICommand BuildSolutionAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        object customIn = null;
                        object customOut = null;
                        const int CommandId = (int)VSConstants.VSStd97CmdID.BuildSln;
                        var dte2 = (EnvDTE80.DTE2)SolutionItem.StorageSolution.DTE;
                        dte2.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), CommandId, ref customIn, ref customOut);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                });
            }
        }

        public ICommand RebuildSolutionAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        object customIn = null;
                        object customOut = null;
                        const int CommandId = (int)VSConstants.VSStd97CmdID.RebuildSln;
                        var dte2 = (EnvDTE80.DTE2)SolutionItem.StorageSolution.DTE;
                        dte2.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), CommandId, ref customIn, ref customOut);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                });
            }
        }

        public ICommand CleanSolutionAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        object customIn = null;
                        object customOut = null;
                        const int CommandId = (int)VSConstants.VSStd97CmdID.CleanSln;
                        var dte2 = (EnvDTE80.DTE2)SolutionItem.StorageSolution.DTE;
                        dte2.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), CommandId, ref customIn, ref customOut);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                });
            }
        }

        public ICommand CancelBuildSolutionAction
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        object customIn = null;
                        object customOut = null;
                        const int CommandId = (int)VSConstants.VSStd97CmdID.CancelBuild;
                        var dte2 = (EnvDTE80.DTE2)SolutionItem.StorageSolution.DTE;
                        dte2.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), CommandId, ref customIn, ref customOut);
                    }
                    catch (Exception ex)
                    {
                        ex.TraceUnknownException();
                    }
                });
            }
        }
    }
}