using Editor.Common;
using Editor.Common.Enums;
using Editor.Content;
using Editor.DLLs;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor.Editors
{
    public class TextureEditor : ViewModelBase, IAssetEditor
    {
        private readonly List<List<List<BitmapSource>>> _sliceBitmaps = new();

        private AssetEditorState _state;
        private Texture _texture = new();
        private List<List<List<Slice>>> _slices;
        private int _arrayIndex;
        private int _mipIndex;
        private int _depthIndex;
        private bool _isRedChannelSet = true;
        private bool _isGreenChannelSet = true;
        private bool _isBlueChannelSet = true;
        private bool _isAlphaChannelSet = true;
        private bool _canSaveChanges;

        public Guid AssetGuid { get; private set; }
        public TextureImportSettings ImportSettings { get; } = new();

        public ICommand SetAllChannelsCommand { get; init; }
        public ICommand SetChannelCommand { get; init; }
        public ICommand RegenerateBitmapsCommand { get; init; }
        public ICommand ReimportCommand { get; init; }
        public ICommand SaveCommand { get; init; }

        Asset IAssetEditor.Asset => Texture;
        public BitmapSource SelectedSliceBitmap =>
            _sliceBitmaps.ElementAtOrDefault(ArrayIndex)?.ElementAtOrDefault(MipIndex)?.ElementAtOrDefault(DepthIndex);

        public Slice SelectedSlice =>
            Texture?.Slices?.ElementAtOrDefault(ArrayIndex)?.ElementAtOrDefault(MipIndex)?.ElementAtOrDefault(DepthIndex);

        public int MaxMipIndex => _sliceBitmaps.Any() && _sliceBitmaps.First().Any() ? _sliceBitmaps.First().Count - 1 : 0;
        public int MaxArrayIndex => _sliceBitmaps.Any() ? _sliceBitmaps.Count - 1 : 0;
        public int MaxDepthIndex =>
            _sliceBitmaps.Any() && _sliceBitmaps.First().Any() && _sliceBitmaps.First().First().Any() ?
                _sliceBitmaps.ElementAtOrDefault(ArrayIndex).ElementAtOrDefault(MipIndex).Count - 1 :
                0;

        public Color Channels => new()
        {
            ScR = IsRedChannelSet ? 1f : 0f,
            ScG = IsGreenChannelSet ? 1f : 0f,
            ScB = IsBlueChannelSet ? 1f : 0f,
            ScA = IsAlphaChannelSet ? 1f : 0f
        };

        public float Stride => (float?)SelectedSliceBitmap?.Format.BitsPerPixel / 8 ?? 1f;
        public long DataSize => Texture?.Slices?.Sum(x => x.Sum(y => y.Sum(z => z.RawContent.LongLength))) ?? 0;

        public AssetEditorState State
        {
            get => _state;
            private set {
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

                    if (Texture is null)
                    {
                        IAssetImportSettings.CopyImportSettings(_texture.ImportSettings, ImportSettings);
                    }

                    OnPropertyChanged(nameof(Texture));
                    SetSelectedBitmap();
                    SetImageChannels();
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
                    SetImageChannels();
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
                    DepthIndex = _depthIndex;
                    OnPropertyChanged(nameof(MipIndex));
                    OnPropertyChanged(nameof(MaxDepthIndex));
                    SetSelectedBitmap();
                    SetImageChannels();
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
                    SetImageChannels();
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
                    SetImageChannels();
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
                    SetImageChannels();
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
                    SetImageChannels();
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
                    SetImageChannels();
                }
            }
        }

        public bool CanSaveChanges
        {
            get => _canSaveChanges;
            set
            {
                if (value != _canSaveChanges)
                {
                    _canSaveChanges = value;
                    OnPropertyChanged(nameof(CanSaveChanges));
                }
            }
        }

        public TextureEditor()
        {
            SetAllChannelsCommand = new RelayCommand<string>(OnSetAllChannelsCommand);
            SetChannelCommand = new RelayCommand<string>(OnSetChannelCommand);
            RegenerateBitmapsCommand = new RelayCommand<bool>(OnRegenerateBitmapsCommand);
            ReimportCommand = new RelayCommand<object>(async x => await OnReimportCommandAsync(x));
            SaveCommand = new RelayCommand<object>(async x => await OnSaveCommandAsync(x));
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
            catch (Exception ex)
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
                OnPropertyChanged(nameof(DataSize));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine($"Failed to load mipmaps from {texture.FileName}");
            }
        }

        private void GenerateSliceBitMaps(bool isNormalMap)
        {
            _sliceBitmaps.Clear();

            foreach (var arraySlice in _slices)
            {
                List<List<BitmapSource>> mipmapsBitmaps = new();

                foreach (var mipLevel in arraySlice)
                {
                    List<BitmapSource> sliceBitmap = new();

                    foreach (var slice in mipLevel)
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

        private void OnSetAllChannelsCommand(object obj)
        {
            _isRedChannelSet = true;
            _isGreenChannelSet = true;
            _isBlueChannelSet = true;
            _isAlphaChannelSet = true;

            OnPropertyChanged(nameof(IsRedChannelSet));
            OnPropertyChanged(nameof(IsGreenChannelSet));
            OnPropertyChanged(nameof(IsBlueChannelSet));
            OnPropertyChanged(nameof(IsAlphaChannelSet));

            SetImageChannels();
        }

        private void OnSetChannelCommand(string obj)
        {
            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                _isRedChannelSet = false;
                _isGreenChannelSet = false;
                _isBlueChannelSet = false;
                _isAlphaChannelSet = false;

                OnPropertyChanged(nameof(IsRedChannelSet));
                OnPropertyChanged(nameof(IsGreenChannelSet));
                OnPropertyChanged(nameof(IsBlueChannelSet));
                OnPropertyChanged(nameof(IsAlphaChannelSet));

                SetImageChannels();
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
            SetImageChannels();
        }

        private void SetImageChannels()
        {
            OnPropertyChanged(nameof(Channels));
            OnPropertyChanged(nameof(Stride));
            OnPropertyChanged(nameof(DataSize));
        }

        private async Task OnReimportCommandAsync(object obj)
        {
            if (Texture is null) return;

            TextureImportSettings settingsBackup = new();

            IAssetImportSettings.CopyImportSettings(Texture.ImportSettings, settingsBackup);
            IAssetImportSettings.CopyImportSettings(ImportSettings, Texture.ImportSettings);

            State = AssetEditorState.IMPORTING;

            bool result = false;

            await Task.Run(() => result = Texture.Import(Texture.FullPath));

            if (result)
            {
                State = AssetEditorState.LOADING;

                await SetMipmapsAsync(Texture);

                SetSelectedBitmap();
                SetImageChannels();

                CanSaveChanges = true;
            }
            else IAssetImportSettings.CopyImportSettings(settingsBackup, Texture.ImportSettings);

            State = AssetEditorState.DONE;
        }

        private async Task OnSaveCommandAsync(object obj)
        {
            if (!CanSaveChanges || Texture is null) return;

            State = AssetEditorState.SAVING;
            CanSaveChanges = false;

            await Task.Run(() => Texture.Save(Texture.FullPath));

            State = AssetEditorState.DONE;
        }
    }
}
