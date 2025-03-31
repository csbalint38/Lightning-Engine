using System.Text.RegularExpressions;

namespace Editor.Utilities
{
    public static class StringExtension
    {
        public static string CamelCaseToSnakeCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            return Regex.Replace(str, "([a-z0-9])([A-Z])", "$1_$2");
        }
    }
}
