using AlekseyNagovitsyn.BuildVision.Tool.Models;
using AlekseyNagovitsyn.BuildVision.Tool.Views;
using BuildVision.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace BuildVision.Common
{
    public static class ProjectStateExtensions
    {
        public static bool IsErrorState(this ProjectState state)
        {
            return state == ProjectState.BuildError || state == ProjectState.CleanError;

        }

        public static ControlTemplate GetAssociatedContent(this ProjectState state)
        {
            var resourcesUri = @"Tool/Views/Resources/ProjectState.Resources.xaml";
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
                case ProjectState.BuildDone:
                case ProjectState.UpToDate:
                case ProjectState.CleanDone:
                    return VectorResources.TryGet(resourcesUri, "BuildDone");
                case ProjectState.BuildError:
                case ProjectState.CleanError:
                    return VectorResources.TryGet(resourcesUri, "BuildError");
                default:
                    return null;
            }
        }
    }
}