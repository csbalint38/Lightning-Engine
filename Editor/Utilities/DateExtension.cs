namespace Editor.Utilities
{
    public static class DateExtension
    {
        public static bool IsOlder(this DateTime date, DateTime other) => date < other;
    }
}
