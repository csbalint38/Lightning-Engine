namespace Editor.Utilities
{
    public interface IUndoRedo
    {
        string Name { get; }
        void Undo();
        void Redo();
    }
}
