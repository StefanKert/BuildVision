using System.Windows;
using System.Windows.Data;

namespace BuildVision.UI.Extensions
{
    public static class BindingExtensions
    {
        public static void UpdateTarget(this FrameworkElement element, DependencyProperty property)
        {
            BindingExpression expression = element.GetBindingExpression(property);
            if (expression != null)
                expression.UpdateTarget();
        }

        public static void UpdateSource(this FrameworkElement element, DependencyProperty property)
        {
            BindingExpression expression = element.GetBindingExpression(property);
            if (expression != null)
                expression.UpdateSource();
        }
    }
}