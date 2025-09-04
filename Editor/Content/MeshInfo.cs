using Editor.Common;

namespace Editor.Content
{
    public class MeshInfo : ViewModelBase
    {
        private byte[] _icon;

        public string Name { get; init; }
        public int IndexCount { get; init; }
        public int VertexCount { get; init; }
        public int TriangleCount { get; init; }

        public byte[] Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }
    }
}
