using Editor.Common.Enums;
using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    class TextureDimensionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            int.TryParse((string)parameter, out var index) && (int)(value as TextureDimension?) == index;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            int.TryParse((string)parameter, out var index) ? (TextureDimension)index : TextureDimension.TEXTURE_2D;
    }
}
