using BuildVision.Contracts;
using System.Windows.Controls;

namespace BuildVision.UI.Extensions
{
    public static class ProjectStateExtensions
    {
        public static bool IsErrorState(this ProjectState state)
        {
            return state == ProjectState.BuildError || state == ProjectState.CleanError;

        }

        public static ControlTemplate GetAssociatedContent(this ProjectState state)
        {
            var resourcesUri = @"Resources/ProjectState.Resources.xaml";
            switch (state)
            {
                case ProjectState.Pending:
                    return VectorResources.TryGet(resourcesUri, "Pending");
                case ProjectState.Skipped:
                case ProjectState.BuildCancelled:
                    return VectorResources.TryGet(resourcesUri, "Skipped");
                case ProjectState.Building:
                case ProjectState.Cleaning:
                    return VectorResources.TryGet(resourcesUri, "Building");
                case ProjectState.UpToDate:
                    return VectorResources.TryGet(resourcesUri, "BuildUpToDate");
                case ProjectState.BuildDone:
                case ProjectState.CleanDone:
                    return VectorResources.TryGet(resourcesUri, "BuildDone");
                case ProjectState.BuildError:
                case ProjectState.CleanError:
                    return VectorResources.TryGet(resourcesUri, "BuildError");
                case ProjectState.BuildWarning:
                    return VectorResources.TryGet(resourcesUri, "BuildWarning");
                default:
                    return null;
            }
        }
    }
}
