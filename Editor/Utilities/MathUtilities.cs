namespace Editor.Utilities
{
    public static class MathUtilities
    {
        public static float Epsilon => .00001f;

        public static bool IsEqual(this float value, float other) => Math.Abs(value - other) < Epsilon;

        public static bool IsEqual(this float? value, float? other)
        {
            if (!value.HasValue || !other.HasValue) return false;

            return IsEqual(value.Value, other.Value);
        }
    }
}
