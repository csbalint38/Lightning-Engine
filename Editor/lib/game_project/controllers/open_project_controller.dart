import 'dart:io';
import "package:collection/collection.dart";
import 'package:editor/common/mvvm/viewmodel.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/game_project/project_data.dart';
import 'package:editor/utilities/logger.dart';

class OpenProjectController extends ViewModelBase {
  static final OpenProjectController _openProjectController =
      OpenProjectController._internal();

  static final Directory _appData =
      Directory("${Platform.environment['APPDATA']!}\\LightningEditor");
  late final File _projectDataFile;
  final List<ProjectData> projects = <ProjectData>[];

  factory OpenProjectController() {
    return _openProjectController;
  }

  OpenProjectController._internal() {
    if (!_appData.existsSync()) {
      _appData.createSync(recursive: false);
    }
    _projectDataFile = File('${_appData.path}\\ProjectData.xml');
    _readProjectData();
  }

  void _readProjectData() {
    if (_projectDataFile.existsSync()) {
      projects.clear();
      ProjectsData projectsData = ProjectsData.fromXMLFile(_projectDataFile);

      for (final project in projectsData.projects) {
        if (File(
                '${project.fullPath}\\${project.projectName}.${Project.extension}')
            .existsSync()) {
          try {
            project.icon = File('${project.fullPath}\\.Lightning\\icon.jpg');
            project.screenshot =
                File('${project.fullPath}\\.Lightning\\screenshot.jpg');
          } catch (e) {
            debugLogger.e(
                "Failed to load screenshot and/or icon for project: ${project.projectName}");
          }
          projects.add(project);
        }
      }
    }
  }

  void _writeProjectData() {
    projects.sort((a, b) => b.lastModified.compareTo(a.lastModified));
    ProjectsData(projects: projects).toXMLFile(_projectDataFile.path);
  }

  Project open(ProjectData projectData) {
    _readProjectData();

    ProjectData? selectedProject = projects.firstWhereOrNull(
      (project) => project.fullPath == projectData.fullPath,
    );

    if (selectedProject != null) {
      selectedProject.lastModified = DateTime.now();
    } else {
      selectedProject = projectData;
      selectedProject.lastModified = DateTime.now();
      projects.add(selectedProject);
    }

    _writeProjectData();

    Project project = Project.load(
      File(
          "${selectedProject.fullPath}\\${selectedProject.projectName}.${Project.extension}"),
    );

    return project;
  }
}
