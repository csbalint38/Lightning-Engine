using Editor.Common;
using Editor.Common.Enums;
using Editor.Content;
using System.Diagnostics;
using System.IO;

namespace Editor.Editors
{
    public class TextureEditor : ViewModelBase, IAssetEditor
    {
        private AssetEditorState _state;
        private Texture _texture;

        public Guid AssetGuid { get; private set; }

        Asset IAssetEditor.Asset => Texture;

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
                }
            }
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

                Texture = texture;
            }
            catch(Exception ex)
            {
                Debug.Write(ex.Message);
                Debug.WriteLine($"Failed to set texture for use in texture editor. File: {info.FullPath}");
                Texture = new();
            }
            finally
            {
                State = AssetEditorState.DONE;
            }
        }
    }
}
