using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public class PascalCaseToSpacedConverter : IValueConverter
    {
        private static readonly Regex NonfirstCapitals = new(@"(?<!^)([A-Z])", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            NonfirstCapitals.Replace((string)value, " $1");

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
