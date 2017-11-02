using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using ProjectItem = BuildVision.UI.Models.ProjectItem;
using BuildVision.Contracts;
using BuildVision.UI.Contracts;
using BuildVision.UI.Extensions;

namespace BuildVision.UI.Helpers
{ 
    public static class BuildImages
    {
        public const string BuildActionResourcesUri = @"Resources/BuildAction.Resources.xaml";
        public const string BuildStateResourcesUri = @"Resources/BuildState.Resources.xaml";

        public static ControlTemplate GetBuildBeginImage(IBuildInfo buildInfo)
        {
            var buildAction = buildInfo.BuildAction;
            var buildScope = buildInfo.BuildScope;

            if (buildAction == null || buildScope == null)
                return null;

            string actionKey = GetBuildActionResourceKey(buildAction.Value);
            return VectorResources.TryGet(BuildActionResourcesUri, actionKey);
        }

        public static ControlTemplate GetBuildDoneImage(IBuildInfo buildInfo, IEnumerable<ProjectItem> allProjects, out ControlTemplate stateImage)
        {
            if (buildInfo?.BuildAction == null || buildInfo?.BuildScope == null)
                throw new ArgumentNullException(nameof(buildInfo));

            if (allProjects == null)
                throw new InvalidOperationException();

            int errorProjectsCount = allProjects.Count(item => item.State.IsErrorState());
            bool buildedProjectsSuccess = buildInfo.BuildedProjects.BuildWithoutErrors;

            string stateKey;

            if (buildedProjectsSuccess)
            {
                if (errorProjectsCount == 0)
                    stateKey = "BuildDone";
                else
                    stateKey = "BuildErrorDone";
            }
            else if (buildInfo.BuildIsCancelled)
                stateKey = "BuildCancelled";
            else
                stateKey = "BuildError";

            stateImage = VectorResources.TryGet(BuildStateResourcesUri, stateKey);

            string actionKey = GetBuildActionResourceKey(buildInfo.BuildAction.Value);
            return VectorResources.TryGet(BuildActionResourcesUri, actionKey);
        }

        private static string GetBuildActionResourceKey(BuildActions buildAction)
        {
            switch (buildAction)
            {
                case BuildActions.BuildActionBuild:
                    return "Build";

                case BuildActions.BuildActionRebuildAll:
                    return "Rebuild";

                case BuildActions.BuildActionClean:
                    return "Clean";

                case BuildActions.BuildActionDeploy:
                    throw new InvalidOperationException();

                default:
                    throw new ArgumentOutOfRangeException(nameof(buildAction));
            }
        }
    }
}
