using System;
using BuildVision.Contracts;
using BuildVision.Exports.Providers;
using BuildVision.Helpers;
using BuildVision.Tool.Models;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Serilog;

namespace BuildVision.Core
{
    public class SolutionBuildEvents : IVsUpdateSolutionEvents2, IVsUpdateSolutionEvents4
    {
        private readonly ISolutionProvider _solutionProvider;
        private readonly IBuildInformationProvider _buildInformationProvider;
        private readonly ILogger _logger;
        private BuildAction _currentBuildAction;

        public SolutionBuildEvents(
            ISolutionProvider solutionProvider,
            IBuildInformationProvider buildInformationProvider,
            ILogger logger)
        {
            _solutionProvider = solutionProvider;
            _buildInformationProvider = buildInformationProvider;
            _logger = logger;
        }

        public void UpdateSolution_BeginUpdateAction(uint dwAction)
        {
            try
            {
                _solutionProvider.ReloadSolution();
                _currentBuildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS)dwAction);
                _buildInformationProvider.BuildStarted(_currentBuildAction, BuildScope.Solution);
                _buildInformationProvider.ReloadCurrentProjects();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "");
                throw;
            }
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate) => VSConstants.S_OK;

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            try
            {
                var projectItem = new UI.Models.ProjectItem();
                var configPair = pCfgProj.ToConfigurationTuple();
                SolutionProjectsExtensions.UpdateProperties(pHierProj.ToProject(), projectItem, configPair.Item1, configPair.Item2);
                var buildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS)dwAction);
                _buildInformationProvider.ProjectBuildStarted(projectItem, buildAction);
                return VSConstants.S_OK;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "");
                throw;
            }
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            try
            {
                var buildAction = StateConverterHelper.ConvertSolutionBuildFlagsToBuildAction(dwAction, (VSSOLNBUILDUPDATEFLAGS)dwAction);
                _buildInformationProvider.ProjectBuildFinished(buildAction, ProjectIdentifierGenerator.GetIdentifierForInteropTypes(pHierProj, pCfgProj), fSuccess == 1, fCancel == 1);
                return VSConstants.S_OK;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "");
                throw;
            }
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            try
            {
                _buildInformationProvider.BuildFinished(fSucceeded == 1, fCancelCommand == 1);
                return VSConstants.S_OK;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "");
                throw;
            }
        }

        #region Interface Implementation
        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;
        public int UpdateSolution_Cancel() => VSConstants.S_OK;
        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;
        public void UpdateSolution_QueryDelayFirstUpdateAction(out int pfDelay) => pfDelay = 0;
        public void UpdateSolution_EndUpdateAction(uint dwAction) { }
        public void OnActiveProjectCfgChangeBatchBegin() { }
        public void OnActiveProjectCfgChangeBatchEnd() { }
        public void UpdateSolution_BeginFirstUpdateAction() { }
        public void UpdateSolution_EndLastUpdateAction() { }
        #endregion
    }
}
