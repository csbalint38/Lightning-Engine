using Editor.Common;
using Editor.Utilities;
using System.Collections.ObjectModel;

namespace Editor.Content.ImportSettingsConfig
{
    public class GeometryImportSettingsConfigurator : ViewModelBase, IImportSettingsConfigurator<GeometryProxy>
    {
        private readonly ObservableCollection<GeometryProxy> _geometryProxies = new();

        public ReadOnlyObservableCollection<GeometryProxy> GeometryProxies { get; }

        public GeometryImportSettingsConfigurator()
        {
            GeometryProxies = new(_geometryProxies);
        }

        public void AddFiles(IEnumerable<string> files, string destinationFolder) =>
            files
                .Except(_geometryProxies.Select(p => p.FileInfo.FullName))
                .ToList()
                .ForEach(f => _geometryProxies.Add(new(f, destinationFolder)));

        public void Import()
        {
            if (!_geometryProxies.Any()) return;

            _ = ContentHelper.ImportFilesAsync(_geometryProxies);
            _geometryProxies.Clear();
        }

        public void RemoveFile(GeometryProxy proxy) => _geometryProxies.Remove(proxy);
    }
}
