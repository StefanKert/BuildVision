using System.Windows.Controls;
using AlekseyNagovitsyn.BuildVision.Tool.Views;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public class AssociatedTemplateAttribute : ProjectStateAttribute
    {
        public ControlTemplate ControlTemplate { get; }

        public AssociatedTemplateAttribute(string resourcesUri, string resourceKey)
        {
            ControlTemplate = VectorResources.TryGet(resourcesUri, resourceKey);
        }
    }
}