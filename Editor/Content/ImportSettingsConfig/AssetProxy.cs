using Editor.Common;
using System.Diagnostics;
using System.IO;

namespace Editor.Content.ImportSettingsConfig
{
    public abstract class AssetProxy : ViewModelBase
    {
        private string? _destinationFolder;

        public FileInfo FileInfo { get; }
        public abstract IAssetImportSettings ImportSettings { get; }

        public string DestinationFolder
        {
            get => _destinationFolder!;
            set
            {
                if (!Path.EndsInDirectorySeparator(value)) value += Path.DirectorySeparatorChar;

                if (_destinationFolder != value)
                {
                    _destinationFolder = value;
                    OnPropertyChanged(nameof(DestinationFolder));
                }
            }
        }

        public AssetProxy(string fileName, string destinationFolder)
        {
            Debug.Assert(File.Exists(fileName));

            FileInfo = new FileInfo(fileName);
            DestinationFolder = destinationFolder;
        }

        public abstract void CopySettings(IAssetImportSettings settings);
    }
}
