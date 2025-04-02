namespace Editor.Components
{
    class MSEntity : MSEntityBase
    {
        public MSEntity(List<Entity> entities) : base(entities)
        {
            Refresh();
        }
    }
}
