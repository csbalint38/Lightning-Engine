using Editor.Common.Enums;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace Editor.Content.ImportSettingsConfig
{
    static class ImportingItemCollection
    {
        private static readonly Lock _lock = new();

        private static AssetType _itemFilter = AssetType.MESH;
        private static ObservableCollection<ImportingItem> _importingItems;

        public static ReadOnlyObservableCollection<ImportingItem> ImportingItems { get; private set; }
        public static CollectionViewSource FilteredItems { get; private set; }

        static ImportingItemCollection()
        {
            _importingItems = new();
            ImportingItems = new(_importingItems);

            FilteredItems = new()
            {
                Source = ImportingItems
            };

            FilteredItems.Filter += (s, e) =>
            {
                var type = (e.Item as ImportingItem).Asset.Type;
                e.Accepted = type == _itemFilter;
            };
        }

        public static void SetItemFilter(AssetType type)
        {
            _itemFilter = type;
            FilteredItems.View.Refresh();
        }

        public static void Add(ImportingItem item)
        {
            lock (_lock)
            {
                Application.Current.Dispatcher.Invoke(() => _importingItems.Add(item));
            }
        }

        public static void Rmove(ImportingItem item)
        {
            lock (_lock)
            {
                Application.Current.Dispatcher.Invoke(() => _importingItems.Remove(item));
            }
        }

        public static void Clear(AssetType type)
        {
            lock (_lock)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in _importingItems.Where(x => x.Asset.Type == type).ToList())
                    {
                        _importingItems.Remove(item);
                    }
                });
            }
        }

        public static ImportingItem? GetItem(Asset asset)
        {
            lock (_lock)
            {
                return _importingItems.FirstOrDefault(x => x.Asset == asset);
            }
        }

        /// <summary>
        ///  Calling this on a UI thread makes sure that all collections are created on the same thread.
        /// </summary>
        public static void Init() { }
    }
}
