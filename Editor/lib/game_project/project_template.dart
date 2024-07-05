import 'dart:io';
import 'package:xml/xml.dart' as xml;

class ProjectTemplate {
  final Directory _templatesDir = Directory("ProjectTemplates");

  late String projectType;
  late String projectFile;
  late List<String> folders;

  late String projectName;
  late File icon;
  late File screenshot;
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
    icon = File(iconPath);
    screenshot = File(screenshotPath);
  }

  factory ProjectTemplate.fromXML(File file) {
    String content = file.readAsStringSync();
    return ProjectTemplate.fromXMLString(content);
  }

  factory ProjectTemplate.fromXMLString(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    String projectType =
        document.findAllElements('ProjectType').single.innerText;
    String projectFile =
        document.findAllElements('ProjectFile').single.innerText;
    var folderNodes =
        document.findAllElements('Folders').single.findElements('String');
    List<String> folders = folderNodes.map((node) => node.innerText).toList();

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
