import 'package:editor/Components/components.dart';
import 'package:vector_math/vector_math.dart';
import 'package:xml/xml.dart' as xml;

class Transform implements Component {
  late Vector3 position;
  late Vector3 rotation;
  late Vector3 scale;

  Transform({Vector3? position, Vector3? rotation, Vector3? scale}) {
    this.position = position ?? Vector3.zero();
    this.rotation = rotation ?? Vector3.zero();
    this.scale = scale ?? Vector3.all(1);
  }

  Transform.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final xml.XmlElement? positionNode = (root.getElement("Position"));
    final xml.XmlElement? rotationNode = (root.getElement("Rotation"));
    final xml.XmlElement? scaleNode = (root.getElement("Scale"));

    final Vector3 position = Vector3(
      double.parse((positionNode?.getElement("X")?.innerText)!),
      double.parse((positionNode?.getElement("Y")?.innerText)!),
      double.parse((positionNode?.getElement("Z")?.innerText)!),
    );

    final Vector3 rotation = Vector3(
      double.parse((rotationNode?.getElement("X")?.innerText)!),
      double.parse((rotationNode?.getElement("Y")?.innerText)!),
      double.parse((rotationNode?.getElement("Z")?.innerText)!),
    );

    final Vector3 scale = Vector3(
      double.parse((scaleNode?.getElement("X")?.innerText)!),
      double.parse((scaleNode?.getElement("Y")?.innerText)!),
      double.parse((scaleNode?.getElement("Z")?.innerText)!),
    );

    Transform(position: position, rotation: rotation, scale: scale);
  }

  @override
  String toXML() {
    final builder = xml.XmlBuilder();
    builder.element("Position", nest: () {
      builder.element("X", nest: position.x.toString());
      builder.element("Y", nest: position.y.toString());
      builder.element("Z", nest: position.z.toString());
    });
    builder.element("Rotation", nest: () {
      builder.element("X", nest: rotation.x.toString());
      builder.element("Y", nest: rotation.y.toString());
      builder.element("Z", nest: rotation.z.toString());
    });
    builder.element("Scale", nest: () {
      builder.element("X", nest: scale.x.toString());
      builder.element("Y", nest: scale.y.toString());
      builder.element("Z", nest: scale.z.toString());
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }
}
