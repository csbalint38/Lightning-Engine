import 'dart:io';
import 'package:editor/game_project/scene.dart';
import 'package:xml/xml.dart' as xml;

class Project {
  static String extension = "lightning";

  final String name;
  final String path;
  final List<Scene> scenes = <Scene>[];

  late String fullPath;

  Project(List<Scene>? scenes, {required this.name, required this.path}) {
    fullPath = '$path\\$name.$extension';
    if (scenes == null) {
      this.scenes.add(Scene(name: "Default Scene"));
    } else {
      for (final Scene scene in scenes) {
        this.scenes.add(scene);
      }
    }
  }

  factory Project.fromXMLFile(File file) {
    String content = file.readAsStringSync();
    return Project.fromXML(content);
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

  String toXML() {
    final builder = xml.XmlBuilder();

    builder.element("Project", nest: () {
      builder.element("Name", nest: name);
      builder.element("Path", nest: path);
      builder.element("Scenes", nest: () {
        for (final scene in scenes) {
          builder.xml(scene.toXML());
        }
      });
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }

  void toXMLFile(String path) {
    final String xmlString = toXML();
    final File file = File(path);
    file.writeAsStringSync(xmlString);
  }
}
