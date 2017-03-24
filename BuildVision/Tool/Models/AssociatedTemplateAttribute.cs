using System.Windows.Controls;
using AlekseyNagovitsyn.BuildVision.Tool.Views;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public class AssociatedTemplateAttribute : ProjectStateAttribute
    {
        private readonly ControlTemplate _controlTemplate;
        public ControlTemplate ControlTemplate
        {
            get { return _controlTemplate; }
        }

        public AssociatedTemplateAttribute(string resourcesUri, string resourceKey)
        {
            _controlTemplate = VectorResources.TryGet(resourcesUri, resourceKey);
        }
    }
}