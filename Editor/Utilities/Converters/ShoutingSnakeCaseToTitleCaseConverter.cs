using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public sealed class ShoutingSnakeCaseToTitleCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameters, CultureInfo culture)
        {
            if(value is null) return string.Empty;

            var s = value.ToString() ?? string.Empty;

            s = s.Replace('_', ' ').ToLowerInvariant();

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
