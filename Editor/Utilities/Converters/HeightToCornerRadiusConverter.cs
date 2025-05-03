using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    class HeightToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            new CornerRadius((double)value / 2);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
