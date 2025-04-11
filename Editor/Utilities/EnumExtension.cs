using System.ComponentModel;

namespace Editor.Utilities
{
    static class EnumExtension
    {
        public static string GetDescription(this Enum value) =>
            (
                value
                    .GetType()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[]
            ).FirstOrDefault()?.Description ?? value.ToString();
    }
}
