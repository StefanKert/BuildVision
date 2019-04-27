using System;
using BuildVision.Contracts;

namespace BuildVision.Extensions
{
    public static class BuildActionsExtensions
    {
        public static ProjectState GetProjectState(this BuildAction buildAction)
        {
            switch (buildAction)
            {
                case BuildAction.Build:
                case BuildAction.RebuildAll:
                    return ProjectState.Building;

                case BuildAction.Clean:
                    return ProjectState.Cleaning;

                case BuildAction.Deploy:
                    throw new InvalidOperationException("vsBuildActionDeploy not supported");

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
        }
    }
}
