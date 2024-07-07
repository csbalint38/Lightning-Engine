import 'package:xml/xml.dart' as xml;

class Scene {
  String name;

  Scene({required this.name});

  factory Scene.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final String name = (root.getElement("Name")?.innerText)!;

    return Scene(name: name);
  }

  String toXML() {
    final builder = xml.XmlBuilder();
    builder.element("Scene", nest: () {
      builder.element("Name", nest: name);
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }
}
