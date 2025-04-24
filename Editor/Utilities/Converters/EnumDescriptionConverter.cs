using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    internal class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Array arr)
            {
                var list = new List<string>();

                foreach (var item in arr)
                {
                    if (item is Enum e) list.Add(e.GetDescription());
                }

                return list;
            }
            else if (value is Enum e) return e.GetDescription();

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
