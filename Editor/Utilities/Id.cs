namespace Editor.Utilities
{
    public static class Id
    {
        public static IdType InvalidId = -1;
        public static bool IsValid(IdType id) => id != InvalidId;
    }
}
