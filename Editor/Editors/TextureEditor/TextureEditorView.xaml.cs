using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for TextureEditorView.xaml
    /// </summary>
    public partial class TextureEditorView : UserControl
    {
        public TextureEditorView()
        {
            InitializeComponent();
            Focus();
        }

        private void centerTexture_Executed(object sender, ExecutedRoutedEventArgs e) => textureView.Center();
        private void zoomInTexture_Executed(object sender, ExecutedRoutedEventArgs e) => textureView.ZoomIn();
        private void zoomOutTexture_Executed(object sender, ExecutedRoutedEventArgs e) => textureView.ZoomOut();
        private void zoomFitTexture_Executed(object sender, ExecutedRoutedEventArgs e) => textureView.ZoomFit();
        private void actualTextureSize_Executed(object sender, ExecutedRoutedEventArgs e) => textureView.ActualSize();
    }
}
