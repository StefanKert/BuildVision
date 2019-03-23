using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Core;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.UI;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Models;
using EnvDTE;
using Microsoft.VisualStudio;

namespace BuildVision.Tool.Building
{
    [Export(typeof(IBuildService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuildManager : IBuildService
    {
        private const string CancelBuildCommand = "Build.Cancel";
        private CancellationTokenSource _buildProcessCancellationToken;
        private bool _buildCancelled;

        public BuildManager()
        {
            //_viewModel.RaiseCommandForSelectedProject += RaiseCommandForSelectedProject;
            //_viewModel.ProjectCopyBuildOutputFilesToClipBoard += ProjectCopyBuildOutputFilesToClipBoard;
            //_viewModel.ShowGeneralSettingsPage += () => _packageContext.ShowOptionPage(typeof(GeneralSettingsDialogPage));
            //_viewModel.ShowGridColumnsSettingsPage += () => _packageContext.ShowOptionPage(typeof(GridSettingsDialogPage));
            //_viewModel.CopyErrorMessageToClipboard += CopyErrorMessageToClipboard;
        }

        public void CancelBuildSolution()
        {
            RaiseCommand(VSConstants.VSStd97CmdID.CancelBuild);
        }

        public void CleanSolution()
        {
            RaiseCommand(VSConstants.VSStd97CmdID.CleanSln);
        }

        public void RebuildSolution()
        {
            RaiseCommand(VSConstants.VSStd97CmdID.RebuildSln);
        }

        public void BuildSolution()
        {
            RaiseCommand(VSConstants.VSStd97CmdID.BuildSln);
        }

        public async Task CancelBuildAsync()
        {
            //if (BuildAction == BuildActions.BuildActionClean)
            //    return;
            //if (CurrentBuildState != BuildState.InProgress || _buildCancelled || _buildCancelledInternally)
            //    return;

            //try
            //{
            //    // We need to create a separate task here because of some weird things that are going on
            //    // when calling ExecuteCommand directly. Directly calling it leads to a freeze. No need 
            //    // for that!
            //    //await _packageContext.ExecuteCommandAsync(CancelBuildCommand);
            //    _buildCancelledInternally = true;
            //}
            //catch (Exception ex)
            //{
            //    ex.Trace("Cancel build failed.");
            //}
        }

        public void RaiseCommandForSelectedProject(UI.Models.ProjectItem selectedProjectItem, int commandId)
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

        private void SelectProjectInSolutionExplorer(UI.Models.ProjectItem projectItem)
        {
            //var solutionExplorer = Services.Dte2.ToolWindows.SolutionExplorer;
            //var project = Services.Dte2.Solution.GetProject(x => x.UniqueName == projectItem.UniqueName);
            //var item = solutionExplorer.FindHierarchyItem(project);
            //if (item == null)
            //    throw new ProjectNotFoundException($"Project '{projectItem.UniqueName}' not found in SolutionExplorer.");
            //solutionExplorer.Parent.Activate();
            //item.Select(vsUISelectionType.vsUISelectionTypeSelect);
        }

        private void ProjectCopyBuildOutputFilesToClipBoard(UI.Models.ProjectItem projItem)
        {
            try
            {
                //Project project = Services.Dte.Solution.GetProject(x => x.UniqueName == projItem.UniqueName);
                //BuildOutputFileTypes fileTypes = null; //_packageContext.ControlSettings.ProjectItemSettings.CopyBuildOutputFileTypesToClipboard;
                //if (fileTypes.IsEmpty)
                //{
                //    MessageBox.Show(@"Nothing to copy: all file types unchecked.", Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                //string[] filePaths = project.GetBuildOutputFilePaths(fileTypes, projItem.Configuration, projItem.Platform).ToArray();
                //if (filePaths.Length == 0)
                //{
                //    MessageBox.Show(@"Nothing copied: selected build output groups are empty.", Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Information);
                //    return;
                //}

                //string[] existFilePaths = filePaths.Where(File.Exists).ToArray();
                //if (existFilePaths.Length == 0)
                //{
                //    string msg = GetCopyBuildOutputFilesToClipboardActionMessage("Nothing copied. {0} wasn't found{1}", filePaths);
                //    MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                //CopyFiles(existFilePaths);

                //if (existFilePaths.Length == filePaths.Length)
                //{
                //    string msg = GetCopyBuildOutputFilesToClipboardActionMessage("Copied {0}{1}", existFilePaths);
                //    MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Information);
                //}
                //else
                //{
                //    string[] notExistFilePaths = filePaths.Except(existFilePaths).ToArray();
                //    string copiedMsg = GetCopyBuildOutputFilesToClipboardActionMessage("Copied {0}{1}", existFilePaths);
                //    string notFoundMsg = GetCopyBuildOutputFilesToClipboardActionMessage("{0} wasn't found{1}", notExistFilePaths);
                //    string msg = string.Concat(copiedMsg, Environment.NewLine, Environment.NewLine, notFoundMsg);
                //    MessageBox.Show(msg, Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Warning);
                //}
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
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                object customIn = null;
                object customOut = null;
                //Services.Dte.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), (int) command, ref customIn, ref customOut);
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }

        public void ShowGridColumnsSettingsPage()
        {
            throw new NotImplementedException();
        }

        public void ShowGeneralSettingsPage()
        {
            throw new NotImplementedException();
        }

        public void ProjectCopyBuildOutputFilesToClipBoard()
        {
            throw new NotImplementedException();
        }

        private void CommandEvents_AfterExecute(string guid, int id, object customIn, object customOut)
        {
            if (id == (int) VSConstants.VSStd97CmdID.CancelBuild
                && Guid.Parse(guid) == VSConstants.GUID_VSStandardCommandSet97)
            {
                //_buildCancelled = true;
                //if (!_buildCancelledInternally)
                //    OnBuildCancelled();
            }
        }

        public void RaiseCommandForSelectedProject()
        {
            throw new NotImplementedException();
        }

        private void NavigateToBuildErrorIfNeeded(UI.Settings.Models.ControlSettings settings)
        {
            // How to get errors that  happend during build?
            //bool navigateToBuildFailureReason = (!BuildedProjects.BuildWithoutErrors
            //                                     && settings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnBuildDone);
            //if (navigateToBuildFailureReason && BuildedProjects.Any(p => p.ErrorsBox.Errors.Any(NavigateToErrorItem)))
            //{
            //    _buildErrorIsNavigated = true;
            //}
        }

        private async void OnErrorRaised(object sender, BuildErrorRaisedEventArgs args)
        {
            //bool buildNeedToCancel = (args.ErrorLevel == ErrorLevel.Error && _viewModel.ControlSettings.GeneralSettings.StopBuildAfterFirstError);
            //if (buildNeedToCancel)
            //    await CancelBuildAsync();

            //bool navigateToBuildFailureReason = (!_buildErrorIsNavigated
            //                                     && args.ErrorLevel == ErrorLevel.Error
            //                                     && _viewModel.ControlSettings.GeneralSettings.NavigateToBuildFailureReason == NavigateToBuildFailureReasonCondition.OnErrorRaised);
            //if (navigateToBuildFailureReason && args.ProjectInfo.ErrorsBox.Errors.Any(NavigateToErrorItem))
            //{
            //    _buildErrorIsNavigated = true;
            //}
        }
    }
}
