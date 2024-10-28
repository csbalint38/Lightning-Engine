import 'dart:io';
import 'dart:typed_data';
import 'package:editor/common/constants.dart';
import 'package:editor/common/relay_command.dart';
import 'package:editor/components/component_factory.dart';
import 'package:editor/components/components.dart';
import 'package:editor/components/game_entity.dart';
import 'package:editor/dll_wrappers/visual_studio.dart';
import 'package:editor/game_project/scene.dart';
import 'package:editor/common/list_notifier.dart';
import 'package:editor/utilities/binary.dart';
import 'package:editor/utilities/logger.dart';
import 'package:editor/utilities/undo_redo.dart';
import 'package:flutter/material.dart';
import 'package:xml/xml.dart' as xml;
import 'package:path/path.dart' as p;

class Project {
  static String extension = "lightning";
  static final UndoRedo _undoRedo = UndoRedo();

  final String name;
  final String path;
  final ListNotifier<Scene> scenes = ListNotifier<Scene>();
  late Scene activeScene;
  final ValueNotifier<BuildConfig> buildConfig =
      ValueNotifier<BuildConfig>(BuildConfig.debug);

  late final String fullPath;
  late final String solution;

  late RelayCommand addScene;
  late RelayCommand removeScene;

  static String getConfigurationName(BuildConfig config) {
    final String value = config.toString().split('.').last;
    return value[0].toUpperCase() + value.substring(1);
  }

  Project(List<Scene>? scenes, {required this.name, required this.path}) {
    fullPath = p.join(path, name, '$name.$extension');
    solution = p.join(path, name, '$name.sln');
    if (scenes == null) {
      this.scenes.add(Scene(name: "Default Scene", isActive: true));
      activeScene = this.scenes.value.first;
    } else {
      for (final Scene scene in scenes) {
        this.scenes.add(scene);
        if (scene.isActive) {
          activeScene = scene;
        }
      }
    }

    addScene = RelayCommand(
      (x) {
        _addScene("New Scene ${this.scenes.value.length}");
        Scene newScene = this.scenes.value.last;
        int index = this.scenes.value.length - 1;
        _undoRedo.add(
          UndoRedoAction(
            name: "Add ${newScene.name}",
            undoAction: () => _removeScene(newScene),
            redoAction: () => this.scenes.insert(index, newScene),
          ),
        );
      },
    );

    removeScene = RelayCommand<Scene>(
      (x) {
        int index = this.scenes.value.indexOf(x);
        _removeScene(x);
        _undoRedo.add(
          UndoRedoAction(
            name: "Remove ${x.name}",
            undoAction: () => this.scenes.insert(index, x),
            redoAction: () => _removeScene(x),
          ),
        );
      },
      (x) => !x.isActive,
    );
  }

  factory Project.fromXMLFile(File file) {
    try {
      String content = file.readAsStringSync();
      Project instance = Project.fromXML(content);

      return instance;
    } catch (err) {
      debugLogger.e(err);
      EditorLogger().log(
        LogLevel.error,
        "Failed to open $file",
        trace: StackTrace.current,
      );
      rethrow;
    }
  }

  factory Project.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final String name = (root.getElement("Name")?.innerText)!;
    final String path = (root.getElement("Path")?.innerText)!;
    final xml.XmlElement scenesNode = (root.getElement("Scenes"))!;
    final List<Scene> scenes = <Scene>[];

    for (final scene in scenesNode.findElements("Scene")) {
      scenes.add(Scene.fromXML(scene.toString()));
    }

    return Project(scenes, name: name, path: path);
  }

  factory Project.load(File file) {
    return Project.fromXMLFile(file);
  }

  String toXML() {
    final builder = xml.XmlBuilder();

    builder.element("Project", nest: () {
      builder.element("Name", nest: name);
      builder.element("Path", nest: path);
      builder.element("Scenes", nest: () {
        for (final scene in scenes.value) {
          builder.xml(scene.toXML());
        }
      });
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }

  void toXMLFile(String path) {
    try {
      final String xmlString = toXML();
      final File file = File(path);
      file.writeAsStringSync(xmlString);
    } catch (err) {
      debugLogger.e(err);
      EditorLogger().log(
        LogLevel.error,
        "Failed to save project to location $path",
        trace: StackTrace.current,
      );
    }
  }

  static void save(Project project) {
    EditorLogger().log(
      LogLevel.info,
      "Saved project to ${project.path}\\${project.name}",
      trace: StackTrace.current,
    );
    return project.toXMLFile(project.fullPath);
  }

  void unload() {
    VisualStudio.closeVisualStudio();
    UndoRedo().resset();
  }

  Future<void> runGame(bool debug) async {
    final BuildConfig config = buildConfig.value == BuildConfig.debug
          ? BuildConfig.debug
          : BuildConfig.release;
    await VisualStudio.buildSolution(this, config, debug);
    if(VisualStudio.buildSucceeded) {
      _saveAsBinary();
      await VisualStudio.run(this, config, debug);
    }
  }

  Future<void> stopGame() async => await VisualStudio.stop();

  void buildGameCodeDll({bool showWindow = true}) async {
    _unloadGameCodeDll();

  await VisualStudio.buildSolution(
      this,
      buildConfig.value == BuildConfig.debug
          ? BuildConfig.debugEditor
          : BuildConfig.releaseEditor,
      showWindow,
    );

    if (VisualStudio.buildSucceeded) {
      _loadGameCodeDll();
    }
  }

  void _saveAsBinary() {
    final BuildConfig config = buildConfig.value == BuildConfig.debug
          ? BuildConfig.debug
          : BuildConfig.release;
    final String configName = config.toString().split('.').last.replaceFirstMapped(RegExp(r'^\w'), (match) => match[0]!.toUpperCase());
    final File bin = File(p.join(path, name, 'x64', configName, 'game.bin'));

    {
      BytesBuilder builder = BytesBuilder();

      builder.add(intToBytes(this.activeScene.entities.value.length)); // Number of entities

      for(final GameEntity entity in activeScene.entities.value) {
        builder.add(intToBytes(0));                                    // Placeholder to entity type
        builder.add(intToBytes(entity.components.value.length));       // Number of components

        for(final Component component in entity.components.value) {
          builder.add(intToBytes(component.toEnumType().index));      // Component type
          component.writeToBinary(builder);                           // Component binary data
        }
      }
    }

    bin.open();
  }

  void _loadGameCodeDll() {}

  void _unloadGameCodeDll() {}

  void _addScene(String sceneName) {
    scenes.add(Scene(name: sceneName));
  }

  void _removeScene(Scene scene) {
    scenes.remove(scene);
  }
}
