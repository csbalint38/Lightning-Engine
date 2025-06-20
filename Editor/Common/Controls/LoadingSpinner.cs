using System.Windows;
using System.Windows.Controls;

namespace Editor.Common.Controls
{
    public class LoadingSpinner : Control
    {

        static LoadingSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LoadingSpinner),
                new FrameworkPropertyMetadata(typeof(LoadingSpinner))
            );
        }
    }
}
