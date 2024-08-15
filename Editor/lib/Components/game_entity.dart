import 'package:editor/Components/components.dart';
import 'package:editor/Components/transform.dart' as lng;
import 'package:editor/common/relay_command.dart';
import 'package:editor/utilities/undo_redo.dart';
import 'package:flutter/material.dart';
import 'package:xml/xml.dart' as xml;

class GameEntity {
  final ValueNotifier<String> name = ValueNotifier<String>("");
  final ValueNotifier<bool> isEnabled = ValueNotifier(false);
  final ValueNotifier<List<Component>> components =
      ValueNotifier<List<Component>>([]);

  late RelayCommand renameEntity;
  late RelayCommand setEntityState;

  GameEntity(List<Component>? components, {required name, isEnabled = true}) {
    this.name.value = name;
    this.isEnabled.value = isEnabled;
    if (components == null ||
        !components.any((element) => element is lng.Transform)) {
      components?.add(lng.Transform());
    }
    this.components.value.addAll(components!);

    renameEntity = RelayCommand<String>(
      (x) {
        String oldName = name;
        _renameEntity(x);

        UndoRedo().add(
          UndoRedoAction(
            name: "Rename GameEntity '$oldName' to '$x'",
            undoAction: () => _renameEntity(oldName),
            redoAction: () => _renameEntity(x),
          ),
        );
      },
      (x) => x != name,
    );

    setEntityState = RelayCommand<bool>((x) {
      _setEntityState(x);

      UndoRedo().add(
        UndoRedoAction(
          name: x ? "Enable GameEntity $name" : "Disable GameEntity $name",
          undoAction: () => _setEntityState(!x),
          redoAction: () => _setEntityState(x),
        ),
      );
    });
  }

  void _renameEntity(String name) {
    this.name.value = name;
  }

  void _setEntityState(bool isEnabled) {
    this.isEnabled.value = isEnabled;
  }

  factory GameEntity.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final String name = (root.getElement("Name")?.innerText)!;
    final bool isEnabled = (root.getElement("IsEnabled")?.innerText) == 'true';
    final xml.XmlElement? componentsNode = root.getElement("Components");
    final List<Component> components = <Component>[];

    if (componentsNode != null) {
      for (final child in componentsNode.childElements) {
        switch (child.name.toString()) {
          case 'Transform':
            components.add(lng.Transform.fromXML(child.toString()));
        }
      }
    }

    return GameEntity(components, name: name, isEnabled: isEnabled);
  }

  String toXML() {
    final builder = xml.XmlBuilder();

    builder.element("GameEntity", nest: () {
      builder.element("Name", nest: name.value);
      builder.element("IsEnabled", nest: isEnabled.value);
      builder.element("Components", nest: () {
        for (final component in components.value) {
          builder.xml(component.toXML());
        }
      });
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }
}
