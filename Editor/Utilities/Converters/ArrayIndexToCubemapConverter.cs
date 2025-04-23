using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public class ArrayIndexToCubemapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index) return index % 6;

            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
