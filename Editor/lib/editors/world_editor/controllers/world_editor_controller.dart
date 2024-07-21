import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/common/mvvm/viewmodel.dart';
import 'package:editor/common/relay_command.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/game_project/scene.dart';
import 'package:editor/utilities/undo_redo.dart';

class WorldEditorController extends ViewModelBase {
  static final WorldEditorController _worldEditorController =
      WorldEditorController._internal();

  late final Project _project;
  final undoRedo = UndoRedo();

  late RelayCommand undo;
  late RelayCommand redo;

  factory WorldEditorController() {
    return _worldEditorController;
  }

  WorldEditorController._internal() {
    undoRedo.undoList.addListener((list) => notify(UndoListChanged()));
    undoRedo.redoList.addListener((list) => notify(RedoListChanged()));

    undo = RelayCommand((x) => undoRedo.undo());
    redo = RelayCommand((x) => undoRedo.redo());
  }

  void setProject(Project project) {
    _project = project;
    _project.scenes.addListener((list) => notify(ScenesListChanged()));
  }

  List<Scene> getScenes() {
    return _project.scenes;
  }

  void addScene() {
    final sceneName = "Scene ${_project.scenes.length}";
    _project.addScene.execute(sceneName);
  }

  void removeScene(int index) {
    final Scene scene = _project.scenes[index];
    _project.removeScene.execute(scene);
  }
}

// EVENTS:

class ScenesListChanged extends ViewEvent {
  ScenesListChanged() : super("ScenesListChanged");
}

class UndoListChanged extends ViewEvent {
  UndoListChanged() : super("UndoListChanged");
}

class RedoListChanged extends ViewEvent {
  RedoListChanged() : super("RedoListChanged");
}
