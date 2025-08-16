using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Common.Controls
{
    internal class ListViewItemEx : ListViewItem
    {
        private bool _defetSelection = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && IsSealed) _defetSelection = true;
            else base.OnMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_defetSelection)
            {
                try
                {
                    base.OnPreviewMouseLeftButtonDown(e);
                }
                finally
                {
                    _defetSelection = false;
                }
            }

            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _defetSelection = false;

            base.OnMouseLeave(e);
        }
    }
}
