using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Editor.Utilities
{
    class EnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                string[] words = enumValue.ToString().Split('_');
                StringBuilder result = new();

                foreach (var word in words)
                {
                    if (word.Length > 0) result.Append(char.ToUpper(word[0]) + word.Substring(1).ToLower());
                }

                return result.ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object _ = null!, CultureInfo __ = null!)
        {
            if (value is string strVal && targetType.IsEnum)
            {
                string enumString = strVal.Replace(" ", "_").ToUpper();

                if (Enum.IsDefined(targetType, enumString)) return Enum.Parse(targetType, enumString);
            }
            throw new ArgumentException("Invalid conversion", nameof(value));
        }
    }
}
