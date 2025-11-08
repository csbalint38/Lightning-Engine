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

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (str.Length == 1) return str.ToUpper();

            return char.ToUpper(str[0]) + str[1..];
        }
    }
}
