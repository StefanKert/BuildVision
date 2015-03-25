using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings;
using AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.BuildProgress;

namespace AlekseyNagovitsyn.BuildVision.Tool.ViewModels
{
    public class BuildProgressViewModel : NotifyPropertyChangedBase
    {
        private readonly ControlSettings _settings;

        public BuildProgressViewModel(ControlSettings settings)
        {
            _settings = settings;
        }

        #region Properties and fields

        private CancellationTokenSource _resetTaskBarInfoCts;

        private int _projectsCount;

        private double _incProgressValue;

        /// <summary>
        /// From 0.0 to 1.0.
        /// </summary>
        private double _actionProgressValue;

        private bool _actionProgressIsMarquee;

        private int _currentQueuePosOfBuildingProject;

        private bool _actionProgressIsError;

        private bool _actionProgressIsVisible;

        private bool _actionProgressIsPaused;

        private readonly Lazy<TaskbarItemInfo> _taskbarItemInfo = new Lazy<TaskbarItemInfo>(() =>
            {
                var window = Application.Current.MainWindow;
                return window.TaskbarItemInfo ?? (window.TaskbarItemInfo = new TaskbarItemInfo());
            });

        /// <summary>
        /// Gets the taskbar interface for the Visual Studio application instance.
        /// </summary>
        private TaskbarItemInfo TaskbarItemInfo
        {
            get { return _taskbarItemInfo.Value; }
        }

        public int CurrentQueuePosOfBuildingProject
        {
            get { return (_currentQueuePosOfBuildingProject); }
        }

        public bool ActionProgressIsVisible
        {
            get { return _actionProgressIsVisible; }
            set
            {
                if (_actionProgressIsVisible != value)
                {
                    _actionProgressIsVisible = value;
                    OnPropertyChanged("ActionProgressIsVisible");
                }
            }
        }

        public bool ActionProgressIsPaused
        {
            get { return _actionProgressIsPaused; }
            set
            {
                if (_actionProgressIsPaused != value)
                {
                    _actionProgressIsPaused = value;
                    OnPropertyChanged("ActionProgressIsPaused");
                }
            }
        }

        #endregion

        private void UpdateTaskBarInfo()
        {
            if (!_settings.GeneralSettings.BuildProgressSettings.TaskBarProgressEnabled)
                return;

            TaskbarItemProgressState state;

            if (!_actionProgressIsVisible)
                state = TaskbarItemProgressState.None;
            else if (_actionProgressIsPaused)
                state = TaskbarItemProgressState.Paused;
            else if (_actionProgressIsMarquee)
                state = TaskbarItemProgressState.Indeterminate;
            else if (_actionProgressIsError)
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

            _currentQueuePosOfBuildingProject = 0;
            _actionProgressValue = 0;
            _actionProgressIsError = false;
            _actionProgressIsMarquee = true;
            ActionProgressIsPaused = false;
            ActionProgressIsVisible = true;

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

            _currentQueuePosOfBuildingProject += 1;
            UpdateTaskBarInfo();
        }

        public void OnBuildProjectDone(bool success)
        {
            if (!success)
                _actionProgressIsError = true;

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

            ActionProgressIsVisible = false;
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
                    throw new ArgumentOutOfRangeException();
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