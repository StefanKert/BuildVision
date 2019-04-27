using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using BuildVision.Contracts;
using BuildVision.Exports.Services;
using BuildVision.Extensions;
using BuildVision.UI.Models;
using BuildVision.Views.Settings;

namespace BuildVision.UI.ViewModels
{
    public class TaskBarInfoService : ITaskBarInfoService
    {
        private CancellationTokenSource _resetTaskBarInfoCts;

        private readonly IPackageSettingsProvider _packageSettingsProvider;

        private readonly Lazy<TaskbarItemInfo> _taskbarItemInfo = new Lazy<TaskbarItemInfo>(() =>
        {
            var window = Application.Current.MainWindow;
            return window.TaskbarItemInfo ?? (window.TaskbarItemInfo = new TaskbarItemInfo());
        });

        public TaskBarInfoService(IPackageSettingsProvider packageSettingsProvider)
        {
            _packageSettingsProvider = packageSettingsProvider;
        }

        public void UpdateTaskBarInfo(BuildState buildState, BuildScope buildScope, int projectsCount, int finishedProjects)
        {
            if (_resetTaskBarInfoCts != null && !_resetTaskBarInfoCts.IsCancellationRequested)
            {
                _resetTaskBarInfoCts.Cancel();
            }

            if (!_packageSettingsProvider.Settings.GeneralSettings.BuildProgressSettings.TaskBarProgressEnabled)
            {
                return;
            }

            _taskbarItemInfo.Value.ProgressState = buildState.ToTaskBarItemProgressState(buildScope); ;

            if (projectsCount <= 0)
            {
                _taskbarItemInfo.Value.ProgressValue = 0;
            }
            else if (projectsCount == finishedProjects)
            {
                _taskbarItemInfo.Value.ProgressValue = 1;
            }
            else
            {
                var incProgressValue = 1.0 / (projectsCount * 2.0);
                _taskbarItemInfo.Value.ProgressValue = incProgressValue * finishedProjects;
            }

            if (buildState == BuildState.Done || buildState == BuildState.ErrorDone || buildState == BuildState.Failed || buildState == BuildState.Cancelled)
            {
                ResetTaskBarInfoOnBuildDone();
            }
        }

        public void ResetTaskBarInfo(bool ifTaskBarProgressEnabled = true)
        {
            if (_taskbarItemInfo.IsValueCreated
                && _packageSettingsProvider.Settings.GeneralSettings.BuildProgressSettings.TaskBarProgressEnabled == ifTaskBarProgressEnabled)
            {
                _taskbarItemInfo.Value.ProgressState = TaskbarItemProgressState.None;
                _taskbarItemInfo.Value.ProgressValue = 0;
            }
        }

        private void ResetTaskBarInfoOnBuildDone()
        {
            var buildProgressSettings = _packageSettingsProvider.Settings.GeneralSettings.BuildProgressSettings;
            if (!buildProgressSettings.TaskBarProgressEnabled)
            {
                return;
            }

            switch (buildProgressSettings.ResetTaskBarProgressAfterBuildDone)
            {
                case ResetTaskBarItemInfoCondition.Never:
                    break;

                case ResetTaskBarItemInfoCondition.Immediately:
                    ResetTaskBarInfo();
                    break;

                case ResetTaskBarItemInfoCondition.AfterDelay:
                    {
                        int delay = buildProgressSettings.ResetTaskBarProgressDelay;
                        if (delay > 0)
                        {
                            _resetTaskBarInfoCts = new CancellationTokenSource();
                            var resetTask = new Task(() => Thread.Sleep(delay), _resetTaskBarInfoCts.Token);
                            _ = resetTask.ContinueWith((tsk, cancelToken) =>
                              {
                                  if (!((CancellationToken)cancelToken).IsCancellationRequested)
                                  {
                                      ResetTaskBarInfo();
                                  }
                              }, _resetTaskBarInfoCts.Token, TaskScheduler.FromCurrentSynchronizationContext());
                            resetTask.Start();
                        }
                        else
                        {
                            ResetTaskBarInfo();
                        }
                    }
                    break;

                case ResetTaskBarItemInfoCondition.ByMouseClick:
                    var window = Application.Current.MainWindow;
                    window.PreviewMouseDown += OnMainWindowTouched;
                    window.LocationChanged += OnMainWindowTouched;
                    window.SizeChanged += OnMainWindowTouched;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildProgressSettings.ResetTaskBarProgressAfterBuildDone));
            }
        }

        private void OnMainWindowTouched(object sender, EventArgs e)
        {
            ResetTaskBarInfo();

            var window = (Window)sender;
            window.PreviewMouseDown -= OnMainWindowTouched;
            window.LocationChanged -= OnMainWindowTouched;
            window.SizeChanged -= OnMainWindowTouched;
        }
    }
}
