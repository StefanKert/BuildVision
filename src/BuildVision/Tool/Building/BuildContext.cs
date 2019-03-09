using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Core;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Extensions;
using BuildVision.UI.Helpers;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models.ToolWindow;
using BuildVision.UI.ViewModels;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorItem = BuildVision.Contracts.ErrorItem;
using ProjectItem = BuildVision.UI.Models.ProjectItem;
using WindowState = BuildVision.UI.Models.WindowState;

namespace BuildVision.Tool.Building
{
    public class BuildContext 
    {
        private const string CancelBuildCommand = "Build.Cancel";
        private CancellationTokenSource _buildProcessCancellationToken;
        private readonly BuildVisionPaneViewModel _viewModel;
        private bool _buildCancelled;

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
            //    await _packageContext.ExecuteCommandAsync(CancelBuildCommand);
            //    _buildCancelledInternally = true;
            //}
            //catch (Exception ex)
            //{
            //    ex.Trace("Cancel build failed.");
            //}
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

        private int GetProjectsCount(int projectsCount)
        {
            // TODO need to figure out how to get building projectscount
            //switch (BuildScope)
            //{
            //    case BuildScopes.BuildScopeSolution:
            //        if (_viewModel.ControlSettings.GeneralSettings.FillProjectListOnBuildBegin)
            //        {
            //            projectsCount = _viewModel.ProjectsList.Count;
            //        }
            //        else
            //        {
            //            try
            //            {
            //                Solution solution = _dte.Solution;
            //                if (solution != null)
            //                    projectsCount = solution.GetProjects().Count;
            //            }
            //            catch (Exception ex)
            //            {
            //                ex.Trace("Unable to count projects in solution.");
            //            }
            //        }
            //        break;

            //    case BuildScopes.BuildScopeBatch:
            //    case BuildScopes.BuildScopeProject:
            //        break;

            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(BuildScope));
            //}

            return projectsCount;
        }

        private void ApplyWindowState(UI.Settings.Models.ControlSettings settings)
        {
            // Figure out when build  is canceled
            //int errorProjectsCount = _viewModel.ProjectsList.Count(item => item.State.IsErrorState());
            //if (errorProjectsCount > 0 || BuildIsCancelled)
            //    ApplyToolWindowStateAction(settings.WindowSettings.WindowActionOnBuildError);
            //else
            //    ApplyToolWindowStateAction(settings.WindowSettings.WindowActionOnBuildSuccess);
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

        private void SetFinalStateForUnfinishedProjects()
        {
            // What happens if project only is builded? What do we do with the projectslist
            //if (BuildScope == BuildScopes.BuildScopeSolution)
            //{
            //    foreach (var projectItem in _viewModel.ProjectsList)
            //    {
            //        if (projectItem.State == ProjectState.Pending)
            //            projectItem.State = ProjectState.Skipped;
            //    }
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

        private bool NavigateToErrorItem(ErrorItem errorItem)
        {
            return false;
            //if (string.IsNullOrEmpty(errorItem?.File) || string.IsNullOrEmpty(errorItem?.ProjectFile))
            //    return false;

            //try
            //{
            //    var projectItem = _viewModel.ProjectsList.FirstOrDefault(x => x.FullName == errorItem.ProjectFile);
            //    if (projectItem == null)
            //        return false;


            //    var project = _dte.Solution.GetProject(x => x.UniqueName == projectItem.UniqueName);
            //    if (project == null)
            //        return false;

            //    return project.NavigateToErrorItem(errorItem);
            //}
            //catch (Exception ex)
            //{
            //    ex.Trace("Navigate to error item exception");
            //    return true;
            //}
        }

        private void ApplyToolWindowStateAction(WindowStateAction windowStateAction)
        {
            //switch (windowStateAction.State)
            //{
            //    case WindowState.Nothing:
            //        break;
            //    case WindowState.Show:
            //        _toolWindowManager.Show();
            //        break;
            //    case WindowState.ShowNoActivate:
            //        _toolWindowManager.ShowNoActivate();
            //        break;
            //    case WindowState.Hide:
            //        _toolWindowManager.Hide();
            //        break;
            //    case WindowState.Close:
            //        _toolWindowManager.Close();
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(windowStateAction));
            //}
        }
    }
}
