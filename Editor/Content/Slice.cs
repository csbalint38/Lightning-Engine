namespace Editor.Content
{
    public class Slice
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RowPitch { get; set; }
        public int SlicePitch { get; set; }
        public byte[] RawContent { get; set; }
    }
}
