import 'dart:io';
import 'package:path/path.dart' as p;
import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/common/mvvm/viewmodel.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/game_project/project_template.dart';
import 'package:editor/utilities/logger.dart';

class NewProjectController extends ViewModelBase {
  static final NewProjectController _newProjectController =
      NewProjectController._internal();

  final Directory _templatesDir = Directory("ProjectTemplates");
  String name = "NewProject";
  String path = p.join(
      Platform.environment['USERPROFILE']!, 'Documents', 'LightningProjects');
  String errorMessage = "";
  final List<ProjectTemplate> _templates = [];
  bool isValid = true;

  factory NewProjectController() {
    return _newProjectController;
  }

  NewProjectController._internal() {
    _getProjectTemplates();
  }

  void setName(name) {
    this.name = name;
    setValidationState(validate());
    notify(NameChanged(name: name));
  }

  void setPath(String path) {
    this.path = path;
    setValidationState(validate());
    notify(PathChanged(path: path));
  }

  void setValidationState(bool isValid) {
    this.isValid = isValid;
    notify(ValidationStateChanged(isValid: isValid));
  }

  void setErrorMessage(String errorMessage) {
    this.errorMessage = errorMessage;
    notify(ErrorMessageChanged(errorMessage: errorMessage));
  }

  List<ProjectTemplate> getTemplates() {
    return _templates;
  }

  bool validate() {
    String path = this.path;

    if (!path.endsWith('\\')) {
      path += '\\';
    }
    path += '$name\\';

    if (name.trim().isEmpty) {
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
    String fullPath = p.join(path, name);

    if (!fullPath.endsWith('\\')) {
      fullPath += '\\';
    }

    Directory projectDir = Directory(fullPath);
    if (!projectDir.existsSync()) {
      await projectDir.create(recursive: true);
    }
    for (final String folder in template.folders) {
      await Directory(p.join(fullPath, folder)).create();
    }
    await Process.run('attrib', ['+h', '$fullPath\\.Lightning']);

    try {
      await template.icon.copy(p.join(fullPath, '.Lightning', 'icon.jpg'));
      await template.screenshot
          .copy(p.join(fullPath, '.Lightning', 'screenshot.jpg'));
    } catch (e) {
      debugLogger.e("Failed to copy icon and/or screenshot from the template");
    }

    String templateString = await File(template.projectFilePath).readAsString();
    templateString =
        templateString.replaceAll('{{0}}', name).replaceAll('{{1}}', path);
    final Project project = Project.fromXML(templateString);
    project.toXMLFile(p.join(fullPath, '$name.${Project.extension}'));
  }

  void _getProjectTemplates() {
    _templatesDir.listSync().forEach((item) {
      if (item is Directory) {
        _templates.add(
          ProjectTemplate.fromXMLFile(
            File(p.join(item.path, 'template.xml')),
          ),
        );
      }
    });
  }
}

// EVENTS:
class NameChanged extends ViewEvent {
  String name;

  NameChanged({required this.name}) : super("NameChanged");
}

class PathChanged extends ViewEvent {
  String path;

  PathChanged({required this.path}) : super("PathChanged");
}

class ValidationStateChanged extends ViewEvent {
  bool isValid;

  ValidationStateChanged({required this.isValid})
      : super("ValidationStateChanged");
}

class ErrorMessageChanged extends ViewEvent {
  String errorMessage;

  ErrorMessageChanged({required this.errorMessage})
      : super("ErrorMessageChanged");
}
