using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.Views;

using EnvDTE;
using ProjectItem = AlekseyNagovitsyn.BuildVision.Tool.Models.ProjectItem;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public static class BuildImages
    {
        private const string BuildActionResourcesUri = @"Tool/Views/Resources/BuildAction.Resources.xaml";
        private const string BuildStateResourcesUri = @"Tool/Views/Resources/BuildState.Resources.xaml";

        public static ControlTemplate GetBuildBeginImage(BuildInfo buildInfo)
        {
            vsBuildAction? buildAction = buildInfo.BuildAction;
            vsBuildScope? buildScope = buildInfo.BuildScope;

            if (buildAction == null || buildScope == null)
                return null;

            string actionKey = GetBuildActionResourceKey(buildAction.Value);
            return VectorResources.TryGet(BuildActionResourcesUri, actionKey);
        }

        public static ControlTemplate GetBuildDoneImage(BuildInfo buildInfo, IEnumerable<ProjectItem> allProjects, out ControlTemplate stateImage)
        {
            if (buildInfo == null || buildInfo.BuildAction == null || buildInfo.BuildScope == null)
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

        private static string GetBuildActionResourceKey(vsBuildAction buildAction)
        {
            switch (buildAction)
            {
                case vsBuildAction.vsBuildActionBuild:
                    return "Build";

                case vsBuildAction.vsBuildActionRebuildAll:
                    return "Rebuild";

                case vsBuildAction.vsBuildActionClean:
                    return "Clean";

                case vsBuildAction.vsBuildActionDeploy:
                    throw new InvalidOperationException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
