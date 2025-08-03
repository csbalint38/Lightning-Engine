namespace Editor.Components
{
    public class MSEntity : MSEntityBase
    {
        public MSEntity(List<Entity> entities) : base(entities)
        {
            Refresh();
        }
    }
}
