using System.Windows;
using System.Windows.Media;

namespace Editor.Utilities
{
    static class VisualExtensions
    {
        public static T? FindVisualParent<T>(this DependencyObject depObject) where T : DependencyObject
        {
            if (!(depObject is Visual)) return null;

            var parent = VisualTreeHelper.GetParent(depObject);

            while (parent is not null)
            {
                if (parent is T type) return type;

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        public static T? FindVisualChild<T>(this DependencyObject depObject) where T : DependencyObject
        {
            if (depObject is not Visual) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObject); i++)
            {
                var child = VisualTreeHelper.GetChild(depObject, i);
                var result = (child as T) ?? FindVisualChild<T>(child);

                if (result is not null) return result;
            }

            return null;
        }
    }
}
