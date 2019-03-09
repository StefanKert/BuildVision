using System;
using System.Threading;
using BuildVision.UI.Common.Logging;

namespace BuildVision.Tool.Building
{
    public class BackgroundBuildProcessUpdater
    {
        private const int BuildInProcessCountOfQuantumSleep = 5;
        private const int BuildInProcessQuantumSleep = 50;
        private object _buildProcessLockObject;

        public BackgroundBuildProcessUpdater()
        {

        }

        public void Start(object state)
        {
            lock (_buildProcessLockObject)
            {
                var token = (CancellationToken) state;
                while (!token.IsCancellationRequested)
                {
                    OnBuildProcess();

                    for (int i = 0; i < BuildInProcessQuantumSleep * BuildInProcessCountOfQuantumSleep; i += BuildInProcessQuantumSleep)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        System.Threading.Thread.Sleep(BuildInProcessQuantumSleep);
                    }
                }
            }
        }


        private void OnBuildProcess()
        {
            try
            {
                //var labelsSettings = _viewModel.ControlSettings.BuildMessagesSettings;
                //string msg = _origTextCurrentState + BuildMessages.GetBuildBeginExtraMessage(this, labelsSettings);

                //_viewModel.TextCurrentState = msg;
                //_statusBarNotificationService.ShowTextWithFreeze(msg);

                //var buildingProjects = BuildingProjects;
                //for (int i = 0; i < buildingProjects.Count; i++)
                //    buildingProjects[i].RaiseBuildElapsedTimeChanged();
            }
            catch (Exception ex)
            {
                ex.TraceUnknownException();
            }
        }
    }
}
