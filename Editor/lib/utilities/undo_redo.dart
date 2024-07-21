import 'package:editor/common/mvvm/observable_list.dart';

abstract class IUndoRedo {
  String get name;
  void undo();
  void redo();
}

class UndoRedoAction implements IUndoRedo {
  @override
  late final String name;
  late final Function undoAction;
  late final Function redoAction;

  UndoRedoAction(
      {required this.name, required this.undoAction, required this.redoAction});

  @override
  void undo() => undoAction();

  @override
  void redo() => redoAction();
}

class UndoRedo {
  static final UndoRedo _undoRedo = UndoRedo._internal();

  factory UndoRedo() {
    return _undoRedo;
  }

  UndoRedo._internal();

  final ObservableList<IUndoRedo> redoList = ObservableList();
  final ObservableList<IUndoRedo> undoList = ObservableList();

  void add(IUndoRedo cmd) {
    undoList.add(cmd);
    redoList.clear();
  }

  void undo() {
    if (undoList.isNotEmpty) {
      var cmd = undoList.last;
      undoList.remove(undoList.last);
      cmd.undo();
      redoList.insert(0, cmd);
    }
  }

  void redo() {
    if (redoList.isNotEmpty) {
      var cmd = redoList.first;
      redoList.removeAt(0);
      cmd.redo();
      undoList.add(cmd);
    }
  }

  void resset() {
    redoList.clear();
    undoList.clear();
  }
}
