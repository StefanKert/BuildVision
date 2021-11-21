using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using BuildVision.Common;
using BuildVision.Common.Logging;
using BuildVision.Contracts;
using BuildVision.Contracts.Exceptions;
using BuildVision.Contracts.Models;
using BuildVision.Exports.Services;
using BuildVision.Helpers;
using BuildVision.UI;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Serilog;
using BuildVision.Extensions;
using EnvDTE80;

namespace BuildVision.Tool.Building
{
    [Export(typeof(IBuildService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BuildService : IBuildService
    {
        private ILogger _logger = LogManager.ForContext<BuildService>();
        private const string CancelBuildCommand = "Build.Cancel";
        private bool _buildCancelledInternally;
        private readonly bool _buildCancelled;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPackageSettingsProvider _packageSettingsProvider;

        [ImportingConstructor]
        public BuildService(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(IPackageSettingsProvider))] IPackageSettingsProvider packageSettingsProvider)
        {
            _serviceProvider = serviceProvider;
            _packageSettingsProvider = packageSettingsProvider;
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

        public async System.Threading.Tasks.Task CancelBuildAsync(IBuildInformationModel buildInformationModel)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (buildInformationModel.BuildAction == BuildAction.Clean)
            {
                return;
            }

            if (buildInformationModel.CurrentBuildState != BuildState.InProgress || _buildCancelled || _buildCancelledInternally)
            {
                return;
            }

            try
            {
                // We need to create a separate task here because of some weird things that are going on
                // when calling ExecuteCommand directly. Directly calling it leads to a freeze. No need 
                // for that!
                var dte = (_serviceProvider.GetService<DTE>() as DTE2);
                dte.ExecuteCommand(CancelBuildCommand);
                _buildCancelledInternally = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Cancel build failed.");
            }
        }

        public void RaiseCommandForSelectedProject(IProjectItem selectedProjectItem, int commandId)
        {
            try
            {
                SelectProjectInSolutionExplorer(selectedProjectItem);
                RaiseCommand((VSConstants.VSStd97CmdID)commandId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Raising command {CommandId} for project {UniqueName} failed.", selectedProjectItem.UniqueName, commandId);
            }
        }

        private void RaiseCommand(VSConstants.VSStd97CmdID command)
        {
            try
            {
                object customIn = null;
                object customOut = null;
                var dte = (_serviceProvider.GetService<DTE>() as DTE2);
                dte.Commands.Raise(VSConstants.GUID_VSStandardCommandSet97.ToString(), (int)command, ref customIn, ref customOut);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Raising command {Command} failed.", command);
            }
        }

        private void SelectProjectInSolutionExplorer(IProjectItem projectItem)
        {
            var solutionExplorer = Core.Services.Dte2.ToolWindows.SolutionExplorer;
            var project = Core.Services.Dte2.Solution.FirstOrDefaultProject(x => x.UniqueName == projectItem.UniqueName);
            var item = solutionExplorer.FindHierarchyItem(project);
            if (item == null)
            {
                throw new ProjectNotFoundException($"Project '{projectItem.UniqueName}' not found in SolutionExplorer.");
            }

            solutionExplorer.Parent.Activate();
            item.Select(vsUISelectionType.vsUISelectionTypeSelect);
        }

        public void ProjectCopyBuildOutputFilesToClipBoard(IProjectItem projItem)
        {
            try
            {
                var project = Core.Services.Dte2.Solution.FirstOrDefaultProject(x => x.UniqueName == projItem.UniqueName);
                var fileTypes = _packageSettingsProvider.Settings.ProjectItemSettings.CopyBuildOutputFileTypesToClipboard;
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
                _logger.Error(ex, "Error copying files to the Clipboard 0x{ErrorCode} ({Message})", ex.ErrorCode, ex.Message);
                MessageBox.Show($"Error copying files to the Clipboard 0x{ex.ErrorCode} ({ex.Message})", Resources.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Copy buildoutput for project {UniqueName} failed.", projItem.UniqueName);
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
                var shortenedFilePaths = FilePathHelper.ShortenPaths(filePaths, MaxFilePathLengthInMessage);
                filesListArg = string.Concat(":", Environment.NewLine, string.Join(Environment.NewLine, shortenedFilePaths));
            }
            else
            {
                filesListArg = ".";
            }

            return string.Format(template, filesCountArg, filesListArg);
        }
    }
}
