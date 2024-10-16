import 'package:editor/Components/components.dart';
import 'package:editor/Components/game_entity.dart';
import 'package:editor/common/relay_command.dart';
import 'package:editor/common/vector_notifier.dart';
import 'package:editor/utilities/undo_redo.dart';
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

  @override
  MSComponent<Component> getMultiselectComponent(MSGameEntity msEntity) =>
      MSTransform(msEntity);
}

enum TransformProperty { position, rotation, scale }

class MSTransform extends MSComponent<Transform> {
  final Vector3Notifier position = Vector3Notifier();
  final Vector3Notifier rotation = Vector3Notifier();
  final Vector3Notifier scale = Vector3Notifier();

  late RelayCommand updateComponentsCommand;

  MSTransform(super.msEntity) {
    updateComponentsCommand = RelayCommand<Map<TransformProperty, Vector3>>((x) {
      print(x['vector']);
      UndoRedo().add(
        UndoRedoAction(
          name: "Something changed-----------$x",
          undoAction: () {
            final MSTransform? newMSTransform = MSGameEntity.getMSGameEntity()?.getComponent<MSTransform>();
            if(newMSTransform != null) {
              newMSTransform.updateComponents(x);
            }
          },
          redoAction: () {
            final MSTransform? newMSTransform = MSGameEntity.getMSGameEntity()?.getComponent<MSTransform>();
            if(newMSTransform != null) {
              newMSTransform.updateComponents(x);
            }
          },
        ),
      );

      updateComponents(x);
    });

    refresh();
    position.addListener(
        () => updateComponentsCommand.execute());
    rotation.addListener(
        () => updateComponentsCommand.execute());
    scale.addListener(
        () => updateComponentsCommand.execute());
  }

  @override
  bool updateComponents(dynamic propertyName) {
    if (propertyName is! TransformProperty) return false;
    switch (propertyName) {
      case TransformProperty.position:
        for (var c in selectedComponents) {
          c.position.setValues(
            position.x ?? c.position.x,
            position.y ?? c.position.y,
            position.z ?? c.position.z,
          );
        }
        return true;
      case TransformProperty.rotation:
        for (var c in selectedComponents) {
          c.rotation.setValues(
            rotation.x ?? c.rotation.x,
            rotation.y ?? c.rotation.y,
            rotation.z ?? c.rotation.z,
          );
        }
        return true;
      case TransformProperty.scale:
        for (var c in selectedComponents) {
          c.rotation.setValues(
            scale.x ?? c.scale.x,
            scale.y ?? c.scale.y,
            scale.z ?? c.scale.z,
          );
        }
        return true;
      default:
        return false;
    }
  }

  @override
  bool updateMSComponent() {
    position.x = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.position.x),
    );
    position.y = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.position.y),
    );
    position.z = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.position.z),
    );
    rotation.x = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.rotation.x),
    );
    rotation.y = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.rotation.y),
    );
    rotation.z = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.rotation.z),
    );
    scale.x = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.scale.x),
    );
    scale.y = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.scale.y),
    );
    scale.z = MSGameEntity.getMixedValue(
      selectedComponents,
      ((x) => x.scale.z),
    );

    return true;
  }
}
