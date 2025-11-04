using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VisualChildrenHelper
{
    public static class VisualChildrenHelper
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        public static IEnumerable<UIElement> FindChildren(DependencyObject depObj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is UIElement)
                {
                    yield return (UIElement)child;
                }

                foreach (var childOfChild in FindChildren(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
