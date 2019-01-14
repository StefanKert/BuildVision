using System;
using System.Diagnostics;
using System.Threading;
using BuildVision.Contracts;
using BuildVision.Tool.Building;
using BuildVision.UI.Common.Logging;
using BuildVision.UI.Contracts;
using BuildVision.UI.ViewModels;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{
    public class SolutionBuildEvents : IVsUpdateSolutionEvents2
    {
        private bool _buildCancelled;
        private bool _buildCancelledInternally;
        private CancellationTokenSource _buildProcessCancellationToken;
        private BuildOutputLogger _buildLogger;
        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");

        public BuildState CurrentBuildState { get; private set; }
        public DateTime BuildStartTime { get; private set; }
        public DateTime BuildFinishTime { get; private set; }
        public BuildActions BuildAction { get; private set; }
        public BuildScopes BuildScope { get; private set; }

        private Project GetProject(IVsHierarchy pHierProj)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            pHierProj.GetProperty(VSConstants.VSITEMID_ROOT, (int) __VSHPROPID.VSHPROPID_ExtObject, out var objProj);
            return objProj as Project;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            Debug.WriteLine($"UpdateSolution_Begin");
            return 0;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            Console.WriteLine($"UpdateSolution_Done: fSucceeded {fSucceeded}, fModified {fModified}, fCancelCommand {fCancelCommand}");
            return 0;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {

            Debug.WriteLine($"UpdateSolution_StartUpdate");
            return 0;
        }

        public int UpdateSolution_Cancel()
        {
            Debug.WriteLine($"UpdateSolution_Cancel");
            return 0;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            var proj = GetProject(pIVsHierarchy);
            Debug.WriteLine($"OnActiveProjectCfgChange {proj.UniqueName}");

            return 0;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            var proj = GetProject(pHierProj);
            Debug.WriteLine($"UpdateProjectCfg_Begin {proj.UniqueName} (IsDirty: {proj.IsDirty}): dwAction {(VSSOLNBUILDUPDATEFLAGS) dwAction} ({dwAction})");
            return 0;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            var projects = _buildLogger.Projects;
            var proj = GetProject(pHierProj);
            Debug.WriteLine($"UpdateProjectCfg_Done {proj.UniqueName} (IsDirty: {proj.IsDirty}): dwAction {(VSSOLNBUILDUPDATEFLAGS)dwAction} ({dwAction}), fSuccess {fSuccess}, fCancel {fCancel}");
            return 0;
        }
    }
}
