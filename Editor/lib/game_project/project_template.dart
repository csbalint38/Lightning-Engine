import 'dart:io';
import 'package:xml/xml.dart' as xml;

class ProjectTemplate {
  final Directory _templatesDir = Directory("ProjectTemplates");

  String projectType;
  String projectFile;
  List<String> folders;

  late String projectName;
  late File icon;
  late File screenshot;
  late String projectFilePath;
  late String iconPath;
  late String screenshotPath;

  ProjectTemplate(
      {required this.projectType,
      required this.projectFile,
      required this.folders}) {
    final Directory templateFolderPath =
        Directory('${_templatesDir.path}/$projectType');
    projectName = _getProjectName();
    iconPath = '${templateFolderPath.path}/icon.png';
    screenshotPath = '${templateFolderPath.path}/screenshot.png';
    projectFilePath = '${templateFolderPath.path}/project.lightning';
    icon = File(iconPath);
    screenshot = File(screenshotPath);
  }

  factory ProjectTemplate.fromXMLFile(File file) {
    String content = file.readAsStringSync();
    return ProjectTemplate.fromXML(content);
  }

  factory ProjectTemplate.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    final xml.XmlElement root = document.rootElement;
    final String projectType = (root.getElement('ProjectType')?.innerText)!;
    final String projectFile = (root.getElement('ProjectFile')?.innerText)!;
    final xml.XmlElement foldersNode = (root.getElement("Folders"))!;
    final List<String> folders = <String>[];

    for (final folder in foldersNode.findElements("Folder")) {
      folders.add(folder.innerText);
    }

    return ProjectTemplate(
        projectType: projectType, projectFile: projectFile, folders: folders);
  }

  String _getProjectName() {
    return projectType
        .replaceAllMapped(
            RegExp(r'([A-Z])'), (Match match) => ' ${match.group(0)}')
        .trim();
  }
}
