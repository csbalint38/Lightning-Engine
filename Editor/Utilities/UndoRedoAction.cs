using System.Diagnostics;

namespace Editor.Utilities
{
    public class UndoRedoAction : IUndoRedo
    {
        private readonly Action _undoAction;
        private readonly Action _redoAction;

        public string Name { get; }

        public UndoRedoAction(string name) => Name = name;

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
