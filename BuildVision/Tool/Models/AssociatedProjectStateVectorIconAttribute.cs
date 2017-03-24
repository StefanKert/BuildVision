namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public class AssociatedProjectStateVectorIconAttribute : AssociatedTemplateAttribute
    {
        public AssociatedProjectStateVectorIconAttribute(string resourceKey)
            : base(@"Tool/Views/ProjectState.Resources.xaml", resourceKey)
        {
        }
    }
}