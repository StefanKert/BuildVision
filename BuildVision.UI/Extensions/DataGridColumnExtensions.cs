using System.Windows;

namespace BuildVision.UI.Extensions
{
    public static class DataGridColumnExtensions
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.RegisterAttached(
            "Name",
            typeof(string),
            typeof(DataGridColumnExtensions));

        /// <summary>
        /// Gets the value of the <see cref="NameProperty"/> dependency property.
        /// </summary>
        public static string GetName(DependencyObject obj)
        {
            return (string)obj.GetValue(NameProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="NameProperty"/> dependency property.
        /// </summary>
        public static void SetName(DependencyObject obj, string value)
        {
            obj.SetValue(NameProperty, value);
        } 
    }
}