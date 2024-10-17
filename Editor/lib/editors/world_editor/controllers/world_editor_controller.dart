import 'package:editor/Components/game_entity.dart';
import 'package:editor/common/relay_command.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/game_project/scene.dart';
import 'package:editor/common/list_notifier.dart';
import 'package:editor/utilities/logger.dart';
import 'package:editor/utilities/undo_redo.dart';
import 'package:flutter/foundation.dart';

class WorldEditorController {
  static final WorldEditorController _worldEditorController =
      WorldEditorController._internal();

  late final Project _project;
  final undoRedo = UndoRedo();

  late RelayCommand undoCommand;
  late RelayCommand redoCommand;
  late RelayCommand saveCommand;
  late RelayCommand renameMultipleCommand;
  late RelayCommand enableMultipleCommand;

  final ValueNotifier<bool> canSave = ValueNotifier(false);
  final ListNotifier<int> selectedEntityIndices = ListNotifier<int>();
  late MSGameEntity msEntity;

  final _logger = EditorLogger();

  factory WorldEditorController() {
    return _worldEditorController;
  }

  WorldEditorController._internal() {
    undoCommand = RelayCommand((x) => undoRedo.undo());
    redoCommand = RelayCommand((x) => undoRedo.redo());
    saveCommand = RelayCommand((x) => save(), (x) => canSave.value);

    renameMultipleCommand = RelayCommand<String>(
      (x) {
        Map<GameEntity, String> oldData = Map.fromIterables(
          List.from(msEntity.selectedEntities),
          msEntity.selectedEntities.map((e) => e.name.value).toList(),
        );

        msEntity.name.value = x;

        UndoRedo().add(
          UndoRedoAction(
            name: "Rename ${msEntity.selectedEntities.length} entities to '$x'",
            undoAction: () {
              for (final data in oldData.entries) {
                data.key.name.value = data.value;
              }
            },
            redoAction: () {
              msEntity.name.value = x;
            },
          ),
        );
      },
      (x) => x != msEntity.name.value,
    );

    enableMultipleCommand = RelayCommand<bool>(
      (x) {
        Map<GameEntity, bool> oldData = Map.fromIterables(
          List.from(msEntity.selectedEntities),
          msEntity.selectedEntities.map((e) => e.isEnabled.value).toList(),
        );

        msEntity.isEnabled.value = x;

        UndoRedo().add(
          UndoRedoAction(
            name:
                "${x ? "'En" : "'Dis"}able' ${msEntity.selectedEntities.length} GameEntities",
            undoAction: () {
              for (final data in oldData.entries) {
                data.key.isEnabled.value = data.value;
              }
            },
            redoAction: () {
              msEntity.isEnabled.value = x;
            },
          ),
        );
      },
      (x) => x != msEntity.isEnabled.value,
    );

    selectedEntityIndices.addListener(_setMsEntity);
    undoRedo.undoList.addListener(_canSaveChanged);
    undoRedo.redoList.addListener(_canSaveChanged);
  }

  EditorLogger get logger => _logger;

  clearLogs() {
    _logger.clear();
  }

  filterLogs(LogLevel filter) {
    List<LogLevel> newFilter = List<LogLevel>.from(_logger.levels);
    _logger.levels.contains(filter)
        ? newFilter.remove(filter)
        : newFilter.add(filter);
    _logger.filterLogs(newFilter);
  }

  _canSaveChanged() {
    canSave.value = true;
  }

  void setProject(Project project) {
    _project = project;
  }

  ValueNotifier<List<Scene>> getScenes() {
    return _project.scenes;
  }

  Scene get getActiveScene => _project.activeScene;

  void addScene() {
    final sceneName = "Scene ${_project.scenes.value.length}";
    _project.addScene.execute(sceneName);
  }

  void removeScene(int index) {
    final Scene scene = _project.scenes.value[index];
    _project.removeScene.execute(scene);
  }

  void addGameEntity(int sceneIndex) {
    final GameEntity entity = GameEntity([], name: "Empty GameEntity");
    _project.scenes.value[sceneIndex].addGameEntity.execute(entity);
  }

  void removeGameEntity(int sceneIndex, GameEntity entity) {
    int index =
        _project.scenes.value[sceneIndex].entities.value.indexOf(entity);
    selectedEntityIndices.remove(index);
    _project.scenes.value[sceneIndex].removeGameEntity.execute(entity);
  }

  void _setMsEntity() {
    final List<GameEntity> selectedEntities = selectedEntityIndices.value
        .map((index) => _project.activeScene.entities.value[index])
        .toList();

    if (selectedEntities.isNotEmpty) {
      msEntity = MSGameEntity(selectedEntities);
    }
  }

  void setMsEntityName(String name) {
    renameMultipleCommand.execute(name);
  }

  void enableMsEntity(bool? isEnabled) {
    enableMultipleCommand.execute(isEnabled);
  }

  void save() {
    Project.save(_project);
    canSave.value = false;
  }

  // modifier:
  // * null - nothing
  // * 0    - Ctrl
  // * 1    - Shift
  void changeSelection(int index, bool? modifier) {
    List<int> prevSelection = List.from(selectedEntityIndices.value);

    // Ctrl
    if (modifier != null && !modifier) {
      if (selectedEntityIndices.value.contains(index)) {
        selectedEntityIndices.remove(index);
      } else {
        selectedEntityIndices.add(index);
      }
    }
    // Shift
    else if (modifier != null && modifier) {
      if (selectedEntityIndices.value.isEmpty) {
        selectedEntityIndices.add(index);
      } else {
        int a = selectedEntityIndices.value[0];
        int b = index;
        if (a > b) {
          a = a ^ b;
          b = a ^ b;
          a = a ^ b;
        }

        final newList = [for (int i = a; i <= b; i++) i];

        selectedEntityIndices.clear(notify: false);
        selectedEntityIndices.addAll(newList);
      }
    }
    // No modifier
    else {
      selectedEntityIndices.clear(notify: false);
      selectedEntityIndices.add(index);
    }
    List<int> currentSelection = List.from(selectedEntityIndices.value);

    if (!listEquals(selectedEntityIndices.value, prevSelection)) {
      undoRedo.add(UndoRedoAction(
        name: "Selection changed",
        undoAction: () {
          selectedEntityIndices.clear(notify: false);
          selectedEntityIndices.addAll(prevSelection);
        },
        redoAction: () {
          selectedEntityIndices.clear(notify: false);
          selectedEntityIndices.addAll(currentSelection);
        },
      ));
    }
  }
}
