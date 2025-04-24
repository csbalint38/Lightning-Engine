using Editor.Common;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Editor.Content.ContentBrowser
{
    public class ContentInfo : ViewModelBase
    {
        public static int IconWidth => 90;
        public byte[] Icon { get; }
        public byte[] IconSmall { get; }
        public string FullPath { get; private set; }
        public string FileName => Path.GetFileNameWithoutExtension(FullPath);
        public bool IsDirectory { get; }
        public DateTime DateModified { get; private set; }
        public long? Size { get; }

        public ICommand RenameCommand { get; private set; }

        public ContentInfo(string fullPath, byte[] icon = null, byte[] smallIcon = null, DateTime? lastModified = null)
        {
            Debug.Assert(File.Exists(fullPath) || Directory.Exists(fullPath));

            var info = new FileInfo(fullPath);

            IsDirectory = ContentHelper.IsDirectory(fullPath);
            DateModified = lastModified ?? info.LastWriteTime;
            Size = IsDirectory ? (long?)null : info.Length;
            Icon = icon;
            IconSmall = smallIcon ?? icon;
            FullPath = fullPath;

            RenameCommand = new RelayCommand<string>(x => Rename(x));
        }

        private void Rename(string newName)
        {
            if (string.IsNullOrEmpty(newName.Trim())) return;

            var extension = IsDirectory ? string.Empty : Asset.AssetFileExtension;
            var path = $@"{Path.GetDirectoryName(FullPath)}{Path.DirectorySeparatorChar}{newName}{extension}";

            if (!Validate(path)) return;

            try
            {
                if (IsDirectory) Directory.Move(FullPath, path);
                else File.Move(FullPath, path);

                FullPath = path;

                var info = new FileInfo(FullPath);

                DateModified = info.LastWriteTime;

                OnPropertyChanged(nameof(FullPath));
                OnPropertyChanged(nameof(DateModified));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private bool Validate(string path)
        {
            var fileName = Path.GetFileName(path);
            var dirName = IsDirectory ? path : Path.GetDirectoryName(path);
            var errorMsg = string.Empty;

            if (!IsDirectory)
            {
                if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                {
                    errorMsg = "Invalid character(s) used in file name.";
                }
                if (File.Exists(path))
                {
                    errorMsg = "File already exists with the same name.";
                }
            }
            else
            {
                if (Directory.Exists(path))
                {
                    errorMsg = "Directory already exists with the same name.";
                }
            }

            if (dirName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                errorMsg = "Invalid character(s) used in path name.";
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return string.IsNullOrEmpty(errorMsg);
        }
    }
}
