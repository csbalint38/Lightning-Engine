using System.Diagnostics;

namespace Editor.Utilities
{
    public class UndoRedoAction : IUndoRedo
    {
        private readonly Action _undoAction;
        private readonly Action _redoAction;

        public string Name { get; }

        public UndoRedoAction(string name) => Name = name;

        public UndoRedoAction(string prop, object instance, object oldValue, object newValue, string name) :
            this(
                name,
                () => instance.GetType().GetProperty(prop).SetValue(instance, oldValue),
                () => instance.GetType().GetProperty(prop).SetValue(instance, newValue)
            )
        { }

        public UndoRedoAction(string name, Action undo, Action redo) : this(name)
        {
            Debug.Assert(undo is not null && redo is not null);

            _undoAction = undo;
            _redoAction = redo;
        }

        public void Redo() => _redoAction();
        public void Undo() => _undoAction();
    }
}
