using System.Diagnostics;

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

        public static long AlignSizeUp(long size, long alignment)
        {
            Debug.Assert(alignment > 0, "Alignment must be non-zero.");

            long mask = alignment - 1;

            Debug.Assert((alignment & mask) == 0, "Alignment should be a power of 2.");

            return ((size + mask) & ~mask);
        }

        public static long AlignSizeDown(long size, long alignment)
        {
            Debug.Assert(alignment > 0, "Alignment must be non-zero.");

            long mask = alignment - 1;

            Debug.Assert((alignment & mask) == 0, "Alignement should be a power of 2.");

            return (size & ~mask);
        }

        public static bool IsPowOf2(int x) => (x != 0) && (x & (x - 1)) == 0;
    }
}
