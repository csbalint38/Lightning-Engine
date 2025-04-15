using Editor.Common;
using Editor.Common.Enums;
using Editor.Content;
using Editor.DLLs;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Editor.Editors
{
    public class TextureEditor : ViewModelBase, IAssetEditor
    {
        private readonly List<List<List<BitmapSource>>> _sliceBitmaps = new();

        private AssetEditorState _state;
        private Texture _texture;
        private List<List<List<Slice>>> _slices;
        private Point _panOffset;
        private double _scaleFactor = 1.0;
        private int _arrayIndex;
        private int _mipIndex;
        private int _depthIndex;
        private bool _isRedChannelSet;
        private bool _isGreenChannelSet;
        private bool _isBlueChannelSet;
        private bool _isAlphaChannelSet;

        public Guid AssetGuid { get; private set; }
        
        public ICommand SetAllChannelsCommand { get; init; }
        public ICommand SetChannelCommand { get; init; }
        public ICommand RegenerateBitmapsCommand { get; init; }

        Asset IAssetEditor.Asset => Texture;
        public BitmapSource SelectedSliceBitmap =>
            _sliceBitmaps.ElementAtOrDefault(ArrayIndex)?.ElementAtOrDefault(MipIndex)?.ElementAtOrDefault(DepthIndex);

        public Slice SelectedSlice =>
            Texture?.Slices?.ElementAtOrDefault(ArrayIndex)?.ElementAtOrDefault(MipIndex)?.ElementAtOrDefault(DepthIndex);

        public int MaxMipIndex => _sliceBitmaps.Any() && _sliceBitmaps.First().Any() ? _sliceBitmaps.First().Count - 1 : 0;
        public int MaxArrayIndex => _sliceBitmaps.Any() ? _sliceBitmaps.Count -1 : 0;
        public int MaxDepthIndex =>
            _sliceBitmaps.Any() && _sliceBitmaps.First().Any() && _sliceBitmaps.First().First().Any() ?
                _sliceBitmaps.ElementAtOrDefault(ArrayIndex).ElementAtOrDefault(MipIndex).Count - 1 :
                0;

        public AssetEditorState State
        {
            get => _state;
            set {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }

        public Texture Texture
        {
            get => _texture;
            private set
            {
                if (_texture != value)
                {
                    _texture = value;
                    OnPropertyChanged(nameof(Texture));
                    SetSelectedBitmap();
                }
            }
        }

        public Point PanOffset
        {
            get => _panOffset;
            set
            {
                if (value != _panOffset)
                {
                    _panOffset = value;
                    OnPropertyChanged(nameof(PanOffset));
                }
            }
        }

        public double ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                if (value != _scaleFactor)
                {
                    _scaleFactor = value;
                    OnPropertyChanged(nameof(ScaleFactor));
                }
            }
        }

        public int ArrayIndex
        {
            get => Math.Min(MaxArrayIndex, _arrayIndex);
            set
            {
                value = Math.Min(value, MaxArrayIndex);
                if (value != _arrayIndex)
                {
                    _arrayIndex = value;
                    OnPropertyChanged(nameof(ArrayIndex));
                    SetSelectedBitmap();
                }
            }
        }

        public int MipIndex
        {
            get => Math.Min(MaxMipIndex, _mipIndex);
            set
            {
                value = Math.Min(value, MaxMipIndex);
                if (value != _mipIndex)
                {
                    _mipIndex = value;
                    OnPropertyChanged(nameof(MipIndex));
                    OnPropertyChanged(nameof(MaxDepthIndex));
                    SetSelectedBitmap();
                }
            }
        }

        public int DepthIndex
        {
            get => Math.Min(MaxDepthIndex, _depthIndex);
            set
            {
                value = Math.Min(value, MaxDepthIndex);
                if (value != _depthIndex)
                {
                    _depthIndex = value;
                    OnPropertyChanged(nameof(DepthIndex));
                    SetSelectedBitmap();
                }
            }
        }

        public bool IsRedChannelSet
        {
            get => _isRedChannelSet;
            set
            {
                if (value != _isRedChannelSet)
                {
                    _isRedChannelSet = value;
                    OnPropertyChanged(nameof(IsRedChannelSet));
                }
            }
        }

        public bool IsGreenChannelSet
        {
            get => _isGreenChannelSet;
            set
            {
                if (value != _isGreenChannelSet)
                {
                    _isGreenChannelSet = value;
                    OnPropertyChanged(nameof(IsGreenChannelSet));
                }
            }
        }

        public bool IsBlueChannelSet
        {
            get => _isBlueChannelSet;
            set
            {
                if (value != _isBlueChannelSet)
                {
                    _isBlueChannelSet = value;
                    OnPropertyChanged(nameof(IsBlueChannelSet));
                }
            }
        }

        public bool IsAlphaChannelSet
        {
            get => _isAlphaChannelSet;
            set
            {
                if (value != _isAlphaChannelSet)
                {
                    _isAlphaChannelSet = value;
                    OnPropertyChanged(nameof(IsAlphaChannelSet));
                }
            }
        }

        public TextureEditor()
        {
            SetAllChannelsCommand = new RelayCommand<string>(OnSetAllChannelsCommand);
            SetChannelCommand = new RelayCommand<string>(OnSetChannelCommand);
            RegenerateBitmapsCommand = new RelayCommand<bool>(OnRegenerateBitmapsCommand);
        }

        public async void SetAssetAsync(AssetInfo info)
        {
            try
            {
                AssetGuid = info.Guid;
                Texture = null;

                Debug.Assert(info is not null && File.Exists(info.FullPath));

                var texture = new Texture();
                
                State = AssetEditorState.LOADING;

                await Task.Run(() =>
                {
                    texture.Load(info.FullPath);
                });

                await SetMipmapsAsync(texture);

                Texture = texture;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine($"Failed to set texture for use in texture editor. File: {info.FullPath}");
                Texture = new();
            }
            finally
            {
                State = AssetEditorState.DONE;
            }
        }

        private async Task SetMipmapsAsync(Texture texture)
        {
            try
            {
                await Task.Run(() =>
                {
                    _slices = texture.ImportSettings.Compress ? ContentToolsAPI.Decompress(texture) : texture.Slices;
                });

                Debug.Assert(_slices?.Any() == true && _slices.First()?.Any() == true);

                GenerateSliceBitMaps(texture.IsNormalMap);
                OnPropertyChanged(nameof(Texture));
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine($"Failed to load mipmaps from {texture.FileName}");
            }
        }

        private void GenerateSliceBitMaps(bool isNormalMap)
        {
            _sliceBitmaps.Clear();

            foreach(var arraySlice in _slices)
            {
                List<List<BitmapSource>> mipmapsBitmaps = new();

                foreach(var mipLevel in arraySlice)
                {
                    List<BitmapSource> sliceBitmap = new();

                    foreach(var slice in mipLevel)
                    {
                        var image = BitmapHelper.ImageFromSlice(slice, isNormalMap);

                        Debug.Assert(image is not null);

                        sliceBitmap.Add(image);
                    }
                    mipmapsBitmaps.Add(sliceBitmap);
                }
                _sliceBitmaps.Add(mipmapsBitmaps);
            }

            OnPropertyChanged(nameof(MaxMipIndex));
            OnPropertyChanged(nameof(MaxArrayIndex));
            OnPropertyChanged(nameof(MaxDepthIndex));
        }

        private void SetSelectedBitmap()
        {
            OnPropertyChanged(nameof(SelectedSliceBitmap));
            OnPropertyChanged(nameof(SelectedSlice));
        }

        private void OnSetAllChannelsCommand(string obj)
        {
            _isRedChannelSet = true;
            _isGreenChannelSet = true;
            _isBlueChannelSet = true;
            _isAlphaChannelSet = true;

            OnPropertyChanged(nameof(IsRedChannelSet));
            OnPropertyChanged(nameof(IsGreenChannelSet));
            OnPropertyChanged(nameof(IsBlueChannelSet));
            OnPropertyChanged(nameof(IsAlphaChannelSet));
        }

        private void OnSetChannelCommand(string obj)
        {
            if(!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _isRedChannelSet = false;
                _isGreenChannelSet = false;
                _isBlueChannelSet = false;
                _isAlphaChannelSet = false;

                OnPropertyChanged(nameof(IsRedChannelSet));
                OnPropertyChanged(nameof(IsGreenChannelSet));
                OnPropertyChanged(nameof(IsBlueChannelSet));
                OnPropertyChanged(nameof(IsAlphaChannelSet));
            }

            switch (obj)
            {
                case "R":
                    IsRedChannelSet = !IsRedChannelSet;
                    break;
                case "G":
                    IsGreenChannelSet = !IsGreenChannelSet;
                    break;
                case "B":
                    IsBlueChannelSet = !IsBlueChannelSet;
                    break;
                case "A":
                    IsAlphaChannelSet = !IsAlphaChannelSet;
                    break;
            }
        }

        private void OnRegenerateBitmapsCommand(bool isNormal) {
            GenerateSliceBitMaps(isNormal);
            OnPropertyChanged(nameof(SelectedSliceBitmap));
        }
    }
}
