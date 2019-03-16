using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

using BuildVision.Common;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.Models;
using BuildVision.UI.Settings.Models.BuildProgress;
using BuildVision.Contracts.Models;
using BuildVision.Contracts;

namespace BuildVision.UI.ViewModels
{ 
    public class BuildProgressViewModel : BindableBase, IBuildProgressViewModel
    {
        private readonly ControlSettings _settings;
        private readonly IBuildInformationModel _buildInformationModel;
        private CancellationTokenSource _resetTaskBarInfoCts;

        private int _projectsCount;

        private double _incProgressValue;

        private double _actionProgressValue;

        private bool _actionProgressIsMarquee;

        private readonly Lazy<TaskbarItemInfo> _taskbarItemInfo = new Lazy<TaskbarItemInfo>(() =>
        {
            var window = Application.Current.MainWindow;
            return window.TaskbarItemInfo ?? (window.TaskbarItemInfo = new TaskbarItemInfo());
        });
        private TaskbarItemInfo TaskbarItemInfo => _taskbarItemInfo.Value;
        public int CurrentQueuePosOfBuildingProject { get; private set; }

        private bool _actionProgressIsPaused;
        public bool ActionProgressIsPaused
        {
            get => _actionProgressIsPaused;
            set => SetProperty(ref _actionProgressIsPaused, value);
        }

        public BuildProgressViewModel(ControlSettings settings, IBuildInformationModel buildInformationModel)
        {
            _settings = settings;
            _buildInformationModel = buildInformationModel;
        }

        private void UpdateTaskBarInfo()
        {
            if (!_settings.GeneralSettings.BuildProgressSettings.TaskBarProgressEnabled)
                return;

            TaskbarItemProgressState state;

            if (_buildInformationModel.CurrentBuildState != BuildState.InProgress)
                state = TaskbarItemProgressState.None;
            else if (_actionProgressIsPaused)
                state = TaskbarItemProgressState.Paused;
            else if (_actionProgressIsMarquee)
                state = TaskbarItemProgressState.Indeterminate;
            else if (_buildInformationModel.CurrentBuildState == BuildState.Failed)
                state = TaskbarItemProgressState.Error;
            else
                state = TaskbarItemProgressState.Normal;

            TaskbarItemInfo.ProgressState = state;
            TaskbarItemInfo.ProgressValue = _actionProgressValue;
        }

        public void ResetTaskBarInfo(bool ifTaskBarProgressEnabled = true)
        {
            if (_taskbarItemInfo.IsValueCreated 
                && _settings.GeneralSettings.BuildProgressSettings.TaskBarProgressEnabled == ifTaskBarProgressEnabled)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                TaskbarItemInfo.ProgressValue = 0;
            }
        }

        public void OnBuildBegin(int projectsCount)
        {
            if (_resetTaskBarInfoCts != null)
                _resetTaskBarInfoCts.Cancel();

            if (projectsCount > 0)
            {
                _projectsCount = projectsCount;
                _incProgressValue = 1.0 / (projectsCount * 2.0);
            }
            else
            {
                _projectsCount = 0;
                _incProgressValue = 0;
            }

            CurrentQueuePosOfBuildingProject = 0;
            _actionProgressValue = 0;
            _actionProgressIsMarquee = true;
            ActionProgressIsPaused = false;

            UpdateTaskBarInfo();
        }

        public void OnBuildProjectBegin()
        {
            if (_projectsCount > 0)
            {
                _actionProgressIsMarquee = false;
                _actionProgressValue += _incProgressValue;
            }
            else
            {
                _actionProgressIsMarquee = true;
            }

            CurrentQueuePosOfBuildingProject += 1;
            UpdateTaskBarInfo();
        }

        public void OnBuildProjectDone(bool success)
        {
            if (_projectsCount > 0)
            {
                _actionProgressIsMarquee = false;
                _actionProgressValue += _incProgressValue;
            }
            else
            {
                _actionProgressIsMarquee = true;
            }

            UpdateTaskBarInfo();
        }

        public void OnBuildDone()
        {
            _actionProgressValue = 1;
            _actionProgressIsMarquee = false;
            UpdateTaskBarInfo();
            _actionProgressValue = 0;
            ResetTaskBarInfoOnBuildDone();
        }

        private void ResetTaskBarInfoOnBuildDone()
        {
            BuildProgressSettings buildProgressSettings = _settings.GeneralSettings.BuildProgressSettings;
            if (!buildProgressSettings.TaskBarProgressEnabled)
                return;

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
                            resetTask.ContinueWith((tsk, cancelToken) =>
                                {
                                    if (!((CancellationToken)cancelToken).IsCancellationRequested)
                                        ResetTaskBarInfo();
                                },
                                _resetTaskBarInfoCts.Token,
                                TaskScheduler.FromCurrentSynchronizationContext());
                            resetTask.Start();
                        }
                        else
                        {
                            ResetTaskBarInfo();
                        }
                    }
                    break;

                case ResetTaskBarItemInfoCondition.ByMouseClick:
                    Window window = Application.Current.MainWindow;
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

        public void OnBuildCancelled()
        {
            ActionProgressIsPaused = true;
            UpdateTaskBarInfo();
        }
    }
}
