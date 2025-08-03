using Editor.Components;
using Editor.Content;
using System.Windows.Controls;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for GeometryComponentView.xaml
    /// </summary>
    public partial class GeometryComponentView : UserControl
    {
        public GeometryComponentView()
        {
            InitializeComponent();
        }

        private void BrdGeometry_Drop(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void BrdGeometry_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 && DataContext is MSGeometry vm && vm.GeometryGuid != Guid.Empty)
            {
                ContentBrowserView.OpenAssetEditor(AssetRegistry.GetAssetInfo(vm.GeometryGuid));
            }
        }
    }
}
