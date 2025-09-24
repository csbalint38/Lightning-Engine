using Editor.Common;
using Editor.Common.Enums;

namespace Editor.Content;

public class Mesh : ViewModelBase
{
    private int _elementSize;
    private int _vertexCount;
    private int _indexSize;
    private int _indexCount;
    private string? _name;

    public static int PositionSize => sizeof(float) * 3;

    public ElementsType ElementsType { get; set; }
    public PrimitiveTopology PrimitiveTopology { get; set; }
    public byte[] Positions { get; set; } = [];
    public byte[] Elements { get; set; } = [];
    public byte[] Indicies { get; set; } = [];

    public int ElementSize
    {
        get => _elementSize;
        set
        {
            if (_elementSize != value)
            {
                _elementSize = value;
                OnPropertyChanged(nameof(ElementSize));
            }
        }
    }

    public int VertexCount
    {
        get => _vertexCount;
        set
        {
            if (_vertexCount != value)
            {
                _vertexCount = value;
                OnPropertyChanged(nameof(VertexCount));
            }
        }
    }

    public int IndexSize
    {
        get => _indexSize;
        set
        {
            if (_indexSize != value)
            {
                _indexSize = value;
                OnPropertyChanged(nameof(IndexSize));
            }
        }
    }

    public int IndexCount
    {
        get => _indexCount;
        set
        {
            if (_indexCount != value)
            {
                _indexCount = value;
                OnPropertyChanged(nameof(IndexCount));
            }
        }
    }

    public string Name
    {
        get => _name!;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }
}
