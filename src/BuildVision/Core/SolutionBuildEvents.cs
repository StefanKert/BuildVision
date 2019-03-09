using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BuildVision.Common;
using BuildVision.Contracts;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.Tool.Building;
using BuildVision.Tool.Models;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.Helpers;
using BuildVision.UI.ViewModels;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{

    public class SolutionBuildEvents : IVsUpdateSolutionEvents2, IVsUpdateSolutionEvents4
    {
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildInformationProvider _buildInformationProvider;
        private readonly IBuildingProjectsProvider _buildingProjectsProvider;

        public SolutionBuildEvents(
            ISolutionProvider solutionProvider, 
            IBuildInformationProvider buildInformationProvider,
            IBuildingProjectsProvider buildingProjectsProvider)
        {
            _solutionProvider = solutionProvider;
            _buildInformationProvider = buildInformationProvider;
            _buildingProjectsProvider = buildingProjectsProvider;
        }

        public void UpdateSolution_BeginUpdateAction(uint dwAction)
        {
            _buildInformationProvider.BuildStarted(dwAction);
            _buildingProjectsProvider.ReloadCurrentProjects();

            //ApplyToolWindowStateAction(_viewModel.ControlSettings.WindowSettings.WindowActionOnBuildBegin);
            //int projectsCount = -1;
            //projectsCount = GetProjectsCount(projectsCount);
            //_viewModel.OnBuildBegin(projectsCount, this);
            //_buildProcessCancellationToken = new CancellationTokenSource();
            // Startbackground process 
            //Task.Factory.StartNew(BuildEvents_BuildInProcess, _buildProcessCancellationToken.Token, _buildProcessCancellationToken.Token);
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            _buildingProjectsProvider.ProjectBuildStarted(pHierProj, pCfgProj, pCfgSln, dwAction);
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            _buildingProjectsProvider.ProjectBuildFinished(pHierProj, pCfgProj, pCfgSln, fSuccess == 1, fCancel == 1);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            _buildInformationProvider.BuildFinished(fSucceeded == 1, fModified == 1, fCancelCommand == 1);

            var result = _buildInformationProvider.GetBuildInformationModel();
            var finishedProjects = _buildingProjectsProvider.GetBuildingProjects();

            return VSConstants.S_OK;
        }

        #region Interface Implementation

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public void UpdateSolution_EndUpdateAction(uint dwAction)
        {
        }

        public void OnActiveProjectCfgChangeBatchBegin()
        {
        }

        public void OnActiveProjectCfgChangeBatchEnd()
        {
        }

        public void UpdateSolution_QueryDelayFirstUpdateAction(out int pfDelay)
        {
            pfDelay = 0;
        }

        public void UpdateSolution_BeginFirstUpdateAction()
        {
        }

        public void UpdateSolution_EndLastUpdateAction()
        {
        }
        #endregion
    }
}
