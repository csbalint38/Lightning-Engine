using Editor.GameProject;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for WorldEditorView.xaml
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();
            Loaded += WorldEditorView_OnLoaded;
        }

        private void WorldEditorView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= WorldEditorView_OnLoaded;
            Focus();
        }
    }
}
