import 'dart:io';
import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/common/mvvm/viewmodel.dart';
import 'package:editor/game_project/project_template.dart';
import 'package:path_provider/path_provider.dart';

class NewProjectController extends ViewModelBase {
  static final NewProjectController _newProjectController =
      NewProjectController._internal();

  final Directory _templatesDir = Directory("ProjectTemplates");
  String name = "NewProject";
  String path = "";
  final List<ProjectTemplate> _templates = [];

  factory NewProjectController() {
    return _newProjectController;
  }

  NewProjectController._internal() {
    _getDocumentsPath();
    _getProjectTemplates();
  }

  void setName(String name) {
    this.name = name;
    notify(NameChanged(name: name));
  }

  void setPath(String path) {
    this.path = path;
    notify(PathChanged(path: path));
  }

  List<ProjectTemplate> getTemplates() {
    return _templates;
  }

  Future<void> _getDocumentsPath() async {
    Directory dir = await getApplicationDocumentsDirectory();
    setPath("${dir.path}\\LightningProjects\\");
  }

  void _getProjectTemplates() {
    _templatesDir.listSync().forEach((item) {
      if (item is Directory) {
        _templates.add(
          ProjectTemplate.fromXML(
            File('${item.path}/template.xml'),
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
