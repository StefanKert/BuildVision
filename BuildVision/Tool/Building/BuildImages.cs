using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.Views;

using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;
using BuildVision.Contracts;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public static class BuildImages
    {
        private const string BuildActionResourcesUri = @"Tool/Views/Resources/BuildAction.Resources.xaml";
        private const string BuildStateResourcesUri = @"Tool/Views/Resources/BuildState.Resources.xaml";

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
            {
                stateImage = null;
                return VectorResources.TryGet(BuildActionResourcesUri, "StandBy");
            }

            if (allProjects == null)
                throw new InvalidOperationException();

            int errorProjectsCount = allProjects.Count(item => item.State.IsErrorState());
            bool buildedProjectsSuccess = buildInfo.BuildedProjects.BuildWithoutErrors;

            string stateKey;
            if (buildInfo.BuildIsCancelled)
                stateKey = "BuildCancelled";
            else if (!buildedProjectsSuccess)
                stateKey = "BuildError";
            else if (buildedProjectsSuccess && errorProjectsCount == 0)
                stateKey = "BuildDone";
            else if (buildedProjectsSuccess && errorProjectsCount != 0)
                stateKey = "BuildErrorDone";
            else
                throw new InvalidOperationException();

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
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
