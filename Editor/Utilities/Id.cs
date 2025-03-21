namespace Editor.Utilities
{
    public static class Id
    {
        public static int InvalidId = -1;
        public static bool IsValid(int id) => id != InvalidId;
    }
}
