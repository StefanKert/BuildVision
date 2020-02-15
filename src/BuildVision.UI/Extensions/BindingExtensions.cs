using System.Windows;

namespace BuildVision.UI.Extensions
{
    public static class BindingExtensions
    {
        public static void UpdateTarget(this FrameworkElement element, DependencyProperty property)
        {
            var expression = element.GetBindingExpression(property);
            expression?.UpdateTarget();
        }

        public static void UpdateSource(this FrameworkElement element, DependencyProperty property)
        {
            var expression = element.GetBindingExpression(property);
            expression?.UpdateSource();
        }
    }
}
