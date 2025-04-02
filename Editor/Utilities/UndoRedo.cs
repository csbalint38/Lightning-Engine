using System.Collections.ObjectModel;

namespace Editor.Utilities
{
    public class UndoRedo
    {
        private bool _registerAction = true;
        private readonly ObservableCollection<IUndoRedo> _redoList = [];
        private readonly ObservableCollection<IUndoRedo> _undoList = [];

        public ReadOnlyObservableCollection<IUndoRedo> RedoList { get; }
        public ReadOnlyObservableCollection<IUndoRedo> UndoList { get; }

        public UndoRedo()
        {
            RedoList = new ReadOnlyObservableCollection<IUndoRedo>(_redoList);
            UndoList = new ReadOnlyObservableCollection<IUndoRedo>(_undoList);
        }

        public void Add(IUndoRedo cmd)
        {
            if (_registerAction)
            {
                _undoList.Add(cmd);
                _redoList.Clear();
            }
        }

        public void Undo()
        {
            if (_undoList.Any())
            {
                var cmd = _undoList.Last();
                _undoList.RemoveAt(_undoList.Count - 1);
                _registerAction = false;
                cmd.Undo();
                _registerAction = true;
                _redoList.Insert(0, cmd);
            }
        }

        public void Redo()
        {
            if (_redoList.Any())
            {
                var cmd = _redoList.First();
                _redoList.RemoveAt(0);
                _registerAction = false;
                cmd.Redo();
                _registerAction = true;
                _undoList.Add(cmd);
            }
        }

        public void Reset()
        {
            _redoList.Clear();
            _undoList.Clear();
        }
    }
}
