using System.ComponentModel;
using System.Reflection;

namespace Editor.Utilities;

static class EnumExtension
{
    public static string GetDescription(this Enum value) =>
        value.GetType().GetField(value.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description
            ?? value.ToString();
}
