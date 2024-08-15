import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:path/path.dart' as p;
import 'package:editor/game_project/project.dart';
import 'package:editor/game_project/project_template.dart';
import 'package:editor/utilities/logger.dart';

class NewProjectController {
  static final NewProjectController _newProjectController =
      NewProjectController._internal();

  final Directory _templatesDir = Directory("ProjectTemplates");
  final ValueNotifier<String> name = ValueNotifier<String>("NewProject");
  final ValueNotifier<String> path = ValueNotifier<String>(p.join(
      Platform.environment['USERPROFILE']!, 'Documents', 'LightningProjects'));
  final ValueNotifier<String> errorMessage = ValueNotifier<String>("");
  final ValueNotifier<List<ProjectTemplate>> templates =
      ValueNotifier<List<ProjectTemplate>>([]);
  final ValueNotifier<bool> isValid = ValueNotifier<bool>(true);

  factory NewProjectController() {
    return _newProjectController;
  }

  NewProjectController._internal() {
    _getProjectTemplates();
  }

  void setName(name) {
    this.name.value = name;
    setValidationState(validate());
  }

  void setPath(String path) {
    this.path.value = path;
    setValidationState(validate());
  }

  void setValidationState(bool isValid) {
    this.isValid.value = isValid;
  }

  void setErrorMessage(String errorMessage) {
    this.errorMessage.value = errorMessage;
  }

  List<ProjectTemplate> getTemplates() {
    return templates.value;
  }

  bool validate() {
    String path = this.path.value;

    if (!path.endsWith('\\')) {
      path += '\\';
    }
    path += '${name.value}\\';

    if (name.value.trim().isEmpty) {
      setErrorMessage("Project name can't be empty or just white spaces");
      return false;
    }

    final RegExp fileInvalidCharacters = RegExp(
        r'^(?!^(?:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\..+)?$)[^\\/:*?"<>|\r\n]{0,254}[^\\/:*?"<>|\r\n. ]$');
    if (fileInvalidCharacters.hasMatch(path)) {
      setErrorMessage("Project name contains invalid characters");
      return false;
    }

    final RegExp pathCharacters = RegExp(
        r'^(?:[a-zA-Z]:\\|\\)(?:[^\\/:*?"<>|\r\n]+\\)*(?:[^\\/:*?"<>|\r\n]*[^\\/:*?"<>|\r\n. ])?$');
    if (!pathCharacters.hasMatch(path)) {
      setErrorMessage("Path contains invalid characters");
      return false;
    }

    Directory dir = Directory(path);
    if (dir.existsSync() && dir.listSync().isNotEmpty) {
      setErrorMessage("Folder alredy exists and its not empty");
      return false;
    }

    setErrorMessage("");
    return true;
  }

  Future<void> createProject(ProjectTemplate template) async {
    String fullPath = p.join(path.value, name.value);

    if (!fullPath.endsWith('\\')) {
      fullPath += '\\';
    }

    try {
      Directory projectDir = Directory(fullPath);
      if (!projectDir.existsSync()) {
        await projectDir.create(recursive: true);
      }
      for (final String folder in template.folders) {
        await Directory(p.join(fullPath, folder)).create();
      }
      await Process.run('attrib', ['+h', '$fullPath\\.Lightning']);
    } catch (err) {
      debugLogger.e(
          "Failed to create project from template. The following error occured:$err");
      EditorLogger().log(
        LogLevel.error,
        "Failed to create project from template.",
        trace: StackTrace.current,
      );
      rethrow;
    }

    try {
      await template.icon.copy(p.join(fullPath, '.Lightning', 'icon.jpg'));
      await template.screenshot
          .copy(p.join(fullPath, '.Lightning', 'screenshot.jpg'));
    } catch (e) {
      debugLogger.e("Failed to copy icon and/or screenshot from the template");
    }

    String templateString = await File(template.projectFilePath).readAsString();
    templateString = templateString
        .replaceAll('{{0}}', name.value)
        .replaceAll('{{1}}', path.value);
    final Project project = Project.fromXML(templateString);
    project.toXMLFile(p.join(fullPath, '${name.value}.${Project.extension}'));
  }

  void _getProjectTemplates() {
    _templatesDir.listSync().forEach((item) {
      if (item is Directory) {
        templates.value.add(
          ProjectTemplate.fromXMLFile(
            File(p.join(item.path, 'template.xml')),
          ),
        );
      }
    });
  }
}
