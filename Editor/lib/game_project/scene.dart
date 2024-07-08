import 'package:xml/xml.dart' as xml;

class Scene {
  String name;
  bool isActive;

  Scene({required this.name, this.isActive = false});

  factory Scene.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final String name = (root.getElement("Name")?.innerText)!;
    final bool isActive =
        (((root.getElement("IsActive")?.innerText)!).toLowerCase() == 'true');

    return Scene(name: name, isActive: isActive);
  }

  String toXML() {
    final builder = xml.XmlBuilder();
    builder.element("Scene", nest: () {
      builder.element("Name", nest: name);
      builder.element("IsActive", nest: isActive.toString());
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }
}
