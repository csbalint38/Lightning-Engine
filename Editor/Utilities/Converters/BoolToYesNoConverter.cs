using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is bool b && b) ? "Yes" : "No";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string s && s.ToLower() == "yes";
    }
}
