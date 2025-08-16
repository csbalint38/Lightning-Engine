using System.Windows;
using System.Windows.Controls;

namespace Editor.Common.Controls
{
    public class ListViewEx : ListView
    {
        protected override DependencyObject GetContainerForItemOverride() => new ListViewItemEx();
    }
}
