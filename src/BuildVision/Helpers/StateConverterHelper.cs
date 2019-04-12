using BuildVision.Contracts;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Tool.Models
{
    public class StateConverterHelper
    {
        public static BuildActions ConvertSolutionBuildFlagsToBuildAction(uint dwAction, VSSOLNBUILDUPDATEFLAGS solutionFlags)
        {
            if (solutionFlags == (VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_NONE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_FORCE_UPDATE))
            {
                return BuildActions.BuildActionRebuildAll;
            }
            else if (solutionFlags == (VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_NONE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_CLEAN))
            {
                return BuildActions.BuildActionClean;
            }
            else if (solutionFlags == (VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_NONE | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD))
            {
                return BuildActions.BuildActionBuild;
            }
            else
            {
                return BuildActions.Unknown;
            }
        }

    }
}
