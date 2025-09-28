using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public class IndexConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2 ||
                values[0] is null ||
                values[0] == DependencyProperty.UnsetValue ||
                values[1] is not IList
            ) return -1;

            return (values[1] as IList)?.IndexOf(values[0]) + 1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
