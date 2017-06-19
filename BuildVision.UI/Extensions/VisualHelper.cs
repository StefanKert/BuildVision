using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace BuildVision.UI.Extensions
{
    public static class VisualHelper
    {
        /// <summary>
        /// Finds the visual child of the given control.
        /// </summary>
        /// <typeparam name="T">The type of control to look for.</typeparam>
        /// <param name="obj">The dependency object.</param>
        /// <returns>The first founded visual child or <c>null</c>.</returns>
        public static T FindVisualChild<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        /// <summary>
        /// Recursively finds the visual children of the given control.
        /// </summary>
        /// <typeparam name="T">The type of control to look for.</typeparam>
        /// <param name="obj">The dependency object.</param>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}