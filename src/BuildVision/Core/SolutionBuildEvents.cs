using System;
using BuildVision.Contracts;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    public class SolutionBuildEvents : IVsUpdateSolutionEvents2, IVsUpdateSolutionEvents4
    {
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildInformationProvider _buildInformationProvider;
        private readonly BuildEvents _buildEvents;
        private BuildAction _currentBuildAction;

        public SolutionBuildEvents(
            ISolutionProvider solutionProvider,
            IBuildInformationProvider buildInformationProvider,
            IServiceProvider serviceProvider)
        {
            _solutionProvider = solutionProvider;
            _buildInformationProvider = buildInformationProvider;
            _buildEvents = (serviceProvider.GetService(typeof(DTE)) as DTE).Events.BuildEvents;
            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
        }

        public void UpdateSolution_BeginUpdateAction(uint dwAction)
        {
            _solutionProvider.ReloadSolution();
            _currentBuildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS)dwAction);
            _buildInformationProvider.ReloadCurrentProjects();
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            // We use the action from UpdateSolution_BeginUpdateAction here because it givs closer details on the current action
            _buildInformationProvider.BuildStarted(_currentBuildAction, (BuildScope)scope);
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            var projectItem = new UI.Models.ProjectItem();
            var configPair = pCfgProj.ToConfigurationTuple();
            SolutionProjectsExtensions.UpdateProperties(pHierProj.ToProject(), projectItem, configPair.Item1, configPair.Item2);
            var buildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS)dwAction);
            _buildInformationProvider.ProjectBuildStarted(projectItem, buildAction);
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            var buildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS)dwAction);
            _buildInformationProvider.ProjectBuildFinished(buildAction, ProjectIdentifierGenerator.GetIdentifierForInteropTypes(pHierProj, pCfgProj), fSuccess == 1, fCancel == 1);
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            _buildInformationProvider.BuildFinished(fSucceeded == 1, fCancelCommand == 1);
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
