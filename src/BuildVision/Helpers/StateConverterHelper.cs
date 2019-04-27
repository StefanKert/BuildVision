using BuildVision.Common.Diagnostics;
using BuildVision.Contracts;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Tool.Models
{
    public class StateConverterHelper
    {
        public static BuildAction ConvertSolutionBuildFlagsToBuildAction(uint dwAction, VSSOLNBUILDUPDATEFLAGS solutionFlags)
        {
            if (solutionFlags == (VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_NONE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_FORCE_UPDATE))
            {
                return BuildAction.RebuildAll;
            }
            else if (solutionFlags == (VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_NONE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_CLEAN))
            {
                return BuildAction.Clean;
            }
            else if (solutionFlags == (VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_NONE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD))
            {
                return BuildAction.Build;
            }
            else
            {
                DiagnosticsClient.TrackTrace($"Received unknown DW Action {dwAction} (SolutionFlags: {solutionFlags}.");
                return BuildAction.Unknown;
            }
        }
    }
}
