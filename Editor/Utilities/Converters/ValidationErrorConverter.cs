using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Editor.Utilities.Converters;

public class ValidationErrorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var errors = value as ReadOnlyObservableCollection<ValidationError>;

        if (errors is not null && errors.Count > 0)
        {
            return errors[0].ErrorContent?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
