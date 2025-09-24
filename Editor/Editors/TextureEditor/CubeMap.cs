using System.Windows.Media.Imaging;

namespace Editor.Editors
{
    public class CubeMap
    {
        public int ArrayIndex { get; set; }
        public int MipIndex { get; set; }
        public BitmapSource? PositiveX { get; set; }
        public BitmapSource? NegativeX { get; set; }
        public BitmapSource? PositiveY { get; set; }
        public BitmapSource? NegativeY { get; set; }
        public BitmapSource? PositiveZ { get; set; }
        public BitmapSource? NegativeZ { get; set; }
    }
}
