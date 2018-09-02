using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

using ProjectItem = BuildVision.UI.Models.ProjectItem;
using ErrorItem = BuildVision.Contracts.ErrorItem;
using WindowState = BuildVision.UI.Models.WindowState;
using Microsoft.VisualStudio;
using System.Windows;
using System.ComponentModel;
using System.IO;
using BuildVision.Contracts;
using EnvDTE80;
using BuildVision.Common;
using BuildVision.UI;

using System.Text;
using BuildVision.Core;
using BuildVision.Helpers;
using BuildVision.Views.Settings;
using BuildVision.Tool.Models;
using BuildVision.UI.Contracts;
using BuildVision.UI.ViewModels;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Models.Indicators.Core;
using BuildVision.UI.Extensions;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models.ToolWindow;
using System.Collections.Specialized;

namespace BuildVision.Tool
{
    public class Tool
    {
        private readonly DTE _dte;
        private readonly DTE2 _dte2;
        private readonly IVsStatusbar _dteStatusBar;
        private readonly ToolWindowManager _toolWindowManager;
        private readonly IBuildInfo _buildContext;
        private readonly IBuildDistributor _buildDistributor;
        private readonly ControlViewModel _viewModel;
        private readonly SolutionEvents _solutionEvents;

        private bool _buildErrorIsNavigated;
        private string _origTextCurrentState;
        private readonly IPackageContext _packageContext;

        public Tool(IPackageContext packageContext, IBuildInfo buildContext, IBuildDistributor buildDistributor, ControlViewModel viewModel)
        {
            _packageContext = packageContext;
            _dte = packageContext.GetDTE();
            _dte2 = packageContext.GetDTE2();
            if (_dte == null)
                throw new InvalidOperationException("Unable to get DTE instance.");

            _dteStatusBar = packageContext.GetStatusBar();
            if (_dteStatusBar == null)
                TraceManager.TraceError("Unable to get IVsStatusbar instance.");

            _toolWindowManager = new ToolWindowManager(packageContext);

            _buildContext = buildContext;
            _buildDistributor = buildDistributor;

            _viewModel = viewModel;
            _solutionEvents = _dte.Events.SolutionEvents;

            Initialize();
        }

        private void Initialize()
        {
            _buildDistributor.OnBuildBegin += (s, e) => BuildEvents_OnBuildBegin();
            _buildDistributor.OnBuildDone += (s, e) => BuildEvents_OnBuildDone();
            _buildDistributor.OnBuildProcess += (s, e) => BuildEvents_OnBuildProcess();
            _buildDistributor.OnBuildCancelled += (s, e) => BuildEvents_OnBuildCancelled();
            _buildDistributor.OnBuildProjectBegin += BuildEvents_OnBuildProjectBegin;
            _buildDistributor.OnBuildProjectDone += BuildEvents_OnBuildProjectDone;
            _buildDistributor.OnErrorRaised += BuildEvents_OnErrorRaised;

            _solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            _solutionEvents.Opened += SolutionEvents_Opened;

            _viewModel.CancelBuildSolution += () => RaiseCommand(VSConstants.VSStd97CmdID.CancelBuild); 
            _viewModel.CleanSolution += () => RaiseCommand(VSConstants.VSStd97CmdID.CleanSln);
            _viewModel.RebuildSolution += () => RaiseCommand(VSConstants.VSStd97CmdID.RebuildSln); 
            _viewModel.BuildSolution += () => RaiseCommand(VSConstants.VSStd97CmdID.BuildSln);
            _viewModel.RaiseCommandForSelectedProject += RaiseCommandForSelectedProject;
            _viewModel.ProjectCopyBuildOutputFilesToClipBoard += ProjectCopyBuildOutputFilesToClipBoard;
            _viewModel.ShowGeneralSettingsPage += () => _packageContext.ShowOptionPage(typeof(GeneralSettingsDialogPage));
            _viewModel.ShowGridColumnsSettingsPage += () => _packageContext.ShowOptionPage(typeof(GridSettingsDialogPage));
            _viewModel.CopyErrorMessageToClipboard += CopyErrorMessageToClipboard;

            UpdateSolutionItem();
        }

        private void CopyErrorMessageToClipboard(ProjectItem projectItem)
        {
            try
            {
                var errors = new StringBuilder();
                foreach(var errorItem in projectItem.ErrorsBox.Errors)
                {
                    errors.AppendLine(string.Format("{0}({1},{2},{3},{4}): error {5}: {6}", errorItem.File, errorItem.LineNumber, errorItem.ColumnNumber, errorItem.EndLineNumber, errorItem.EndColumnNumber, errorItem.Code, errorItem.Message));
                }
                Clipboard.SetText(errors.ToString());
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void SolutionEvents_Opened()
        {
            UpdateSolutionItem();
            _viewModel.ResetIndicators(ResetIndicatorMode.ResetValue);
        }

        private void SolutionEvents_AfterClosing()
        {
            _viewModel.TextCurrentState = Resources.BuildDoneText_BuildNotStarted;
            _viewModel.ImageCurrentState = VectorResources.TryGet(BuildImages.BuildActionResourcesUri, "StandBy");
            _viewModel.ImageCurrentStateResult = null;

            UpdateSolutionItem();
            _viewModel.ProjectsList.Clear();
            _viewModel.ResetIndicators(ResetIndicatorMode.Disable);
            _viewModel.BuildProgressViewModel.ResetTaskBarInfo();
        }

        private void RaiseCommandForSelectedProject(ProjectItem selectedProjectItem, int commandId)
        {
            try
            {
                SelectProjectInSolutionExplorer(selectedProjectItem);
                RaiseCommand((VSConstants.VSStd97CmdID) commandId);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void SelectProjectInSolutionExplorer(ProjectItem projectItem)
        {
            var solutionExplorer = _dte2.ToolWindows.SolutionExplorer;
            var project = _dte.Solution.GetProject(x => x.UniqueName == projectItem.UniqueName);
            var item = solutionExplorer.FindHierarchyItem(project);
            if (item == null)
                throw new ProjectNotFoundException($"Project '{projectItem.UniqueName}' not found in SolutionExplorer.");
            solutionExplorer.Parent.Activate();
            item.Select(vsUISelectionType.vsUISelectionTypeSelect);
        }
        
        private void ProjectCopyBuildOutputFilesToClipBoard(ProjectItem projItem)
        {
            try
            {
                Project project = _dte.Solution.GetProject(x => x.UniqueName == projItem.UniqueName);
                BuildOutputFileTypes fileTypes = _packageContext.ControlSettings.ProjectItemSettings.CopyBuildOutputFileTypesToClipboard;
                if (fileTypes.IsEmpty)
                {
                    MessageBox.Show(@"Nothing to copy: all file types unchecked.", Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string[] filePaths = project.GetBuildOutputFilePaths(fileTypes, projItem.Configuration, projItem.Platform).ToArray();
                if (filePaths.Length == 0)
                {
                    MessageBox.Show(@"Nothing copied: selected build output groups are empty.", Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string[] existFilePaths = filePaths.Where(File.Exists).ToArray();
                if (existFilePaths.Length == 0)
                {
                    string msg = GetCopyBuildOutputFilesToClipboardActionMessage("Nothing copied. {0} wasn't found{1}", filePaths);
                    MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CopyFiles(existFilePaths);

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
                string msg = string.Format("Error copying files to the Clipboard: 0x{0:X} ({1})", ex.ErrorCode, ex.Message);
                ex.Trace(msg);
                MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
                MessageBox.Show(ex.Message, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CopyFiles(string[] files)
        {
            var fileCollection = new StringCollection();
            fileCollection.AddRange(files);
            Clipboard.Clear();
            Clipboard.SetFileDropList(fileCollection);
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

        private void RaiseCommand(VSConstants.VSStd97CmdID command)
        {
            try
            {
                object customIn = null;
                object customOut = null;
                _dte.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), (int)command, ref customIn, ref customOut);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void UpdateSolutionItem()
        {
            ViewModelHelper.UpdateSolution(_dte.Solution, _viewModel.SolutionItem);
        }

        private void BuildEvents_OnBuildProjectBegin(object sender, BuildProjectEventArgs e)
        {
            try
            {
                ProjectItem currentProject = e.ProjectItem;
                currentProject.State = e.ProjectState;
                currentProject.BuildFinishTime = null;
                currentProject.BuildStartTime = e.EventTime;

                _viewModel.OnBuildProjectBegin();
                if (_buildContext.BuildScope == BuildScopes.BuildScopeSolution &&
                    (_buildContext.BuildAction == BuildActions.BuildActionBuild ||
                     _buildContext.BuildAction == BuildActions.BuildActionRebuildAll))
                {
                    currentProject.BuildOrder = _viewModel.BuildProgressViewModel.CurrentQueuePosOfBuildingProject;
                }

                if (!_viewModel.ProjectsList.Contains(currentProject))
                    _viewModel.ProjectsList.Add(currentProject);
                else if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                    _viewModel.OnPropertyChanged(nameof(ControlViewModel.GroupedProjectsList));
                _viewModel.CurrentProject = currentProject;
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void BuildEvents_OnBuildProjectDone(object sender, BuildProjectEventArgs e)
        {
            if (e.ProjectState == ProjectState.BuildError && _viewModel.ControlSettings.GeneralSettings.StopBuildAfterFirstError)
                _buildDistributor.CancelBuild();

            try
            {
                ProjectItem currentProject = e.ProjectItem;
                currentProject.State = e.ProjectState;
                currentProject.BuildFinishTime = DateTime.Now;
                currentProject.UpdatePostBuildProperties(e.BuildedProjectInfo);

                if (!_viewModel.ProjectsList.Contains(currentProject))
                    _viewModel.ProjectsList.Add(currentProject);

                var buildInfo = (IBuildInfo)sender;
                if (ReferenceEquals(_viewModel.CurrentProject, e.ProjectItem) && buildInfo.BuildingProjects.Any())
                    _viewModel.CurrentProject = buildInfo.BuildingProjects.Last();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }

            _viewModel.UpdateIndicators(_buildContext);

            try
            {
                _viewModel.OnBuildProjectDone(e.BuildedProjectInfo);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void BuildEvents_OnBuildBegin()
        {
            try
            {
                _buildErrorIsNavigated = false;

                ApplyToolWindowStateAction(_viewModel.ControlSettings.WindowSettings.WindowActionOnBuildBegin);

                UpdateSolutionItem();

                string message = BuildMessages.GetBuildBeginMajorMessage(_viewModel.SolutionItem, _buildContext, _viewModel.ControlSettings.BuildMessagesSettings);

                OutputInStatusBar(message, true);
                _viewModel.TextCurrentState = message;
                _origTextCurrentState = message;
                _viewModel.ImageCurrentState = BuildImages.GetBuildBeginImage(_buildContext);
                _viewModel.ImageCurrentStateResult = null;

                ViewModelHelper.UpdateProjects(_viewModel.SolutionItem, _dte.Solution);
                _viewModel.ProjectsList.Clear();

                if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                    _viewModel.ProjectsList.AddRange(_viewModel.SolutionItem.AllProjects);

                _viewModel.ResetIndicators(ResetIndicatorMode.ResetValue);

                OnBuildBegin(_buildContext.BuildScope);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public void OnBuildBegin(BuildScopes? buildScope)
        {
            int projectsCount = -1;
            switch (buildScope)
            {
                case BuildScopes.BuildScopeSolution:
                    if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
                    {
                        projectsCount = _viewModel.ProjectsList.Count;
                    }
                    else
                    {
                        try
                        {
                            Solution solution = _dte.Solution;
                            if (solution != null)
                                projectsCount = solution.GetProjects().Count;
                        }
                        catch (Exception ex)
                        {
                            ex.Trace("Unable to count projects in solution.");
                        }
                    }
                    break;

                case BuildScopes.BuildScopeBatch:
                case BuildScopes.BuildScopeProject:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildScope));
            }

            _viewModel.OnBuildBegin(projectsCount, _buildContext);
        }


        private void OutputInStatusBar(string str, bool freeze)
        {
            if (!_viewModel.ControlSettings.GeneralSettings.EnableStatusBarOutput) 
                return;

            if (_dteStatusBar == null)
                return;

            _dteStatusBar.FreezeOutput(0);
            _dteStatusBar.SetText(str);
            if (freeze)
                _dteStatusBar.FreezeOutput(1);
        }

        private void BuildEvents_OnBuildProcess()
        {
            try
            {
                var labelsSettings = _viewModel.ControlSettings.BuildMessagesSettings;
                string msg = _origTextCurrentState + BuildMessages.GetBuildBeginExtraMessage(_buildContext, labelsSettings);

                _viewModel.TextCurrentState = msg;
                OutputInStatusBar(msg, true);

                var buildingProjects = _buildContext.BuildingProjects;
                for (int i = 0; i < buildingProjects.Count; i++)
                    buildingProjects[i].RaiseBuildElapsedTimeChanged();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void BuildEvents_OnBuildDone()
        {
            try
            {
                var settings = _viewModel.ControlSettings;

                if (_buildContext.BuildScope == BuildScopes.BuildScopeSolution)
                {
                    foreach (var projectItem in _viewModel.ProjectsList)
                    {
                        if (projectItem.State == ProjectState.Pending)
                            projectItem.State = ProjectState.Skipped;
                    }
                }

                _viewModel.UpdateIndicators(_buildContext);

                var message = BuildMessages.GetBuildDoneMessage(_viewModel.SolutionItem, _buildContext, settings.BuildMessagesSettings);
                var buildDoneImage = BuildImages.GetBuildDoneImage(_buildContext, _viewModel.ProjectsList, out ControlTemplate stateImage);

                OutputInStatusBar(message, false);
                _viewModel.TextCurrentState = message;
                _viewModel.ImageCurrentState = buildDoneImage;
                _viewModel.ImageCurrentStateResult = stateImage;
                _viewModel.CurrentProject = null;
                _viewModel.OnBuildDone(_buildContext);

                int errorProjectsCount = _viewModel.ProjectsList.Count(item => item.State.IsErrorState());
                if (errorProjectsCount > 0 || _buildContext.BuildIsCancelled)
                    ApplyToolWindowStateAction(settings.WindowSettings.WindowActionOnBuildError);
                else
                    ApplyToolWindowStateAction(settings.WindowSettings.WindowActionOnBuildSuccess);

                bool navigateToBuildFailureReason = (!_buildContext.BuildedProjects.BuildWithoutErrors
                                                     && settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnBuildDone);
                if (navigateToBuildFailureReason && _buildContext.BuildedProjects.Any(p => p.ErrorsBox.Errors.Any(NavigateToErrorItem)))
                {
                    _buildErrorIsNavigated = true;
                }
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        private void BuildEvents_OnBuildCancelled()
        {
            _viewModel.OnBuildCancelled(_buildContext);
        }

        private void BuildEvents_OnErrorRaised(object sender, BuildErrorRaisedEventArgs args)
        {
            bool buildNeedToCancel = (args.ErrorLevel == ErrorLevel.Error && _viewModel.ControlSettings.GeneralSettings.StopBuildAfterFirstError);
            if (buildNeedToCancel)
                _buildDistributor.CancelBuild();

            bool navigateToBuildFailureReason = (!_buildErrorIsNavigated
                                                 && args.ErrorLevel == ErrorLevel.Error
                                                 && _viewModel.ControlSettings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnErrorRaised);
            if (navigateToBuildFailureReason && args.ProjectInfo.ErrorsBox.Errors.Any(NavigateToErrorItem))
            {
                _buildErrorIsNavigated = true;
            }
        }

        private bool NavigateToErrorItem(ErrorItem errorItem)
        {
            if (string.IsNullOrEmpty(errorItem?.File) || string.IsNullOrEmpty(errorItem?.ProjectFile))
                return false;

            try
            {
                var projectItem = _viewModel.ProjectsList.FirstOrDefault(x => x.FullName == errorItem.ProjectFile);
                if (projectItem == null)
                    return false;


                var project = _dte.Solution.GetProject(x => x.UniqueName == projectItem.UniqueName);
                if (project == null)
                    return false;

                return project.NavigateToErrorItem(errorItem);
            }
            catch (Exception ex)
            {
                ex.Trace("Navigate to error item exception");
                return true;
            }
        }

        private void ApplyToolWindowStateAction(WindowStateAction windowStateAction)
        {
            switch (windowStateAction.State)
            {
                case WindowState.Nothing:
                    break;
                case WindowState.Show:
                    _toolWindowManager.Show();
                    break;
                case WindowState.ShowNoActivate:
                    _toolWindowManager.ShowNoActivate();
                    break;
                case WindowState.Hide:
                    _toolWindowManager.Hide();
                    break;
                case WindowState.Close:
                    _toolWindowManager.Close();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(windowStateAction));
            }
        }
    }
}
