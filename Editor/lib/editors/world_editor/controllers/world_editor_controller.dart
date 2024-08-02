import 'package:editor/Components/game_entity.dart';
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

  late RelayCommand undoCommand;
  late RelayCommand redoCommand;
  late RelayCommand saveCommand;

  bool _canSave = false;

  factory WorldEditorController() {
    return _worldEditorController;
  }

  WorldEditorController._internal() {
    undoRedo.undoList.addListener((list) => notify(UndoListChanged()));
    undoRedo.redoList.addListener((list) => notify(RedoListChanged()));
    undoRedo.undoList.addListener((list) {
      _canSave = true;
    });
    undoRedo.redoList.addListener((list) {
      _canSave = true;
    });

    undoCommand = RelayCommand((x) => undoRedo.undo());
    redoCommand = RelayCommand((x) => undoRedo.redo());
    saveCommand = RelayCommand((x) => save(), (x) => _canSave);
  }

  void setProject(Project project) {
    _project = project;
    _project.scenes.addListener((list) => notify(ScenesListChanged()));
    _project.activeScene.entities
        .addListener((list) => notify(ActiveSceneEntitiesListChanged()));
  }

  List<Scene> getScenes() {
    return _project.scenes;
  }

  Scene get getActiveScene => _project.activeScene;

  void addScene() {
    final sceneName = "Scene ${_project.scenes.length}";
    _project.addScene.execute(sceneName);
  }

  void removeScene(int index) {
    final Scene scene = _project.scenes[index];
    _project.removeScene.execute(scene);
  }

  void addGameEntity(int sceneIndex) {
    final GameEntity entity = GameEntity([], name: "Empty GameEntity");
    _project.scenes[sceneIndex].addGameEntity.execute(entity);
  }

  void removeGameEntity(int sceneIndex, GameEntity entity) {
    _project.scenes[sceneIndex].removeGameEntity.execute(entity);
  }

  void save() {
    Project.save(_project);
    _canSave = false;
    notify(ProjectSaved());
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

class ProjectSaved extends ViewEvent {
  ProjectSaved() : super("ProjectSaved");
}

class ActiveSceneEntitiesListChanged extends ViewEvent {
  ActiveSceneEntitiesListChanged() : super("ActiveSceneEntitiesListChanged");
}

class SelectedEntityIndexChanged extends ViewEvent {
  int index;

  SelectedEntityIndexChanged({required this.index})
      : super("SelectedEntityIndexChanged");
}
