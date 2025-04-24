using Editor.Common;
using Editor.GameProject;
using Editor.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Editor.Content.ImportSettingsConfig
{
    public class ConfigureImportSettings : ViewModelBase
    {
        public string LastDestinationFolder { get; private set; }
        public GeometryImportSettingsConfigurator GeometryImportSettingsConfigurator { get; } = new();
        public TextureImportSettingsConfigurator TextureImportSettingsConfigurator { get; } = new();
        public AudioImportSettingConfigurator AudioImportSettingsConfigurator { get; } = new();

        public int FileCount =>
            GeometryImportSettingsConfigurator.GeometryProxies.Count +
            TextureImportSettingsConfigurator.TextureProxies.Count +
            0; //AudioImportSettingsConfigurator.AudioProxies.Count;

        public ConfigureImportSettings(string[] files, string destinationFolder)
        {
            AddFiles(files, destinationFolder);
            LastDestinationFolder = destinationFolder;
        }

        public ConfigureImportSettings(string destinationFolder)
        {
            Debug.Assert(!string.IsNullOrEmpty(destinationFolder) && Directory.Exists(destinationFolder));

            if (!destinationFolder.EndsWith(Path.DirectorySeparatorChar)) destinationFolder += Path.DirectorySeparatorChar;

            LastDestinationFolder = destinationFolder;

            Debug.Assert(Application.Current.Dispatcher.Invoke(() => destinationFolder.Contains(Project.Current.ContentPath)));
        }

        public void Import()
        {
            GeometryImportSettingsConfigurator.Import();
            TextureImportSettingsConfigurator.Import();
            //AudioImportSettingsConfigurator.Import();
        }

        public void AddFiles(string[] files, string destinationFolder)
        {
            Debug.Assert(files is not null);
            Debug.Assert(!string.IsNullOrEmpty(destinationFolder) && Directory.Exists(destinationFolder));

            if (!destinationFolder.EndsWith(Path.DirectorySeparatorChar)) destinationFolder += Path.DirectorySeparatorChar;

            Debug.Assert(Application.Current.Dispatcher.Invoke(() => destinationFolder.Contains(Project.Current.ContentPath)));

            LastDestinationFolder = destinationFolder;

            var meshFiles = files.Where(f => ContentHelper.MeshFileExtensions.Contains(Path.GetExtension(f).ToLower()));
            var imageFiles = files.Where(f => ContentHelper.ImageFileExtensions.Contains(Path.GetExtension(f).ToLower()));
            //var audioFiles = files.Where(f => ContentHelper.AudioFileExtension.Contains(Path.GetExtension(f).ToLower()));

            GeometryImportSettingsConfigurator.AddFiles(meshFiles, destinationFolder);
            TextureImportSettingsConfigurator.AddFiles(imageFiles, destinationFolder);
            //AudioImportSettingsConfigurator.AddFiles(audioFiles, destinationFolder);
        }
    }
}
