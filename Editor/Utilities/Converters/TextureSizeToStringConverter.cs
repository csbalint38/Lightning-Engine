using Editor.Editors;
using System.Globalization;
using System.Windows.Data;

namespace Editor.Utilities.Converters
{
    public class TextureSizeToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is TextureEditor editor && editor.Texture is not null && editor.SelectedSlice is not null)
            {
                var texture = editor.Texture;
                var size = $"{texture.Width}x{texture.Height}";
                var mipSize = $"({editor.SelectedSlice.Width}x{editor.SelectedSlice.Height})";

                if (texture.IsVolumeMap)
                {
                    size += $"x{texture.Slices[0][0].Count}";
                    mipSize += $"{texture.Slices[0][editor.MipIndex].Count}";
                }

                return $"{size} {mipSize}";
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
