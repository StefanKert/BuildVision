using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildVision.Contracts
{
    public enum BuildActions
    {
        BuildActionBuild = 1,
        BuildActionRebuildAll = 2,
        BuildActionClean = 3,
        BuildActionDeploy = 4
    }
}
