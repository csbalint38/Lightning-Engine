using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public class ReciprocalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double n && double.IsNormal(n)) return 1.0 / n;

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            Convert(value, targetType, parameter, culture);
    }
}
