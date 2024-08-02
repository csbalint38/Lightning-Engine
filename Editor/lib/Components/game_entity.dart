import 'package:editor/Components/components.dart';
import 'package:editor/Components/transform.dart';
import 'package:editor/common/mvvm/observable_list.dart';
import 'package:xml/xml.dart' as xml;

class GameEntity {
  String name;
  final ObservableList<Component> components = ObservableList<Component>();

  GameEntity(List<Component>? components, {required this.name}) {
    if (components == null ||
        !components.any((element) => element is Transform)) {
      components?.add(Transform());
    }
    this.components.addAll(components!);
  }

  factory GameEntity.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final String name = (root.getElement("Name")?.innerText)!;
    final xml.XmlElement? componentsNode = root.getElement("Components");
    final List<Component> components = <Component>[];

    if (componentsNode != null) {
      for (final child in componentsNode.childElements) {
        switch (child.name.toString()) {
          case 'Transform':
            components.add(Transform.fromXML(child.toString()));
        }
      }
    }

    return GameEntity(components, name: name);
  }

  String toXML() {
    final builder = xml.XmlBuilder();

    builder.element("GameEntity", nest: () {
      builder.element("Name", nest: name);
      builder.element("Components", nest: () {
        for (final component in components) {
          builder.xml(component.toXML());
        }
      });
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }
}
