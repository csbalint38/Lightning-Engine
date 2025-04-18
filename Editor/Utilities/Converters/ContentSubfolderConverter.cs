using Editor.GameProject;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    class ContentSubfolderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var contentFolder = Project.Current.ContentPath;

            if(value is string folder && !string.IsNullOrEmpty(folder) && folder.Contains(contentFolder))
            {
                return $@"{Path.DirectorySeparatorChar}{folder.Replace(contentFolder, "")}";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
