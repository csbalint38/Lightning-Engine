import 'package:editor/common/relay_command.dart';
import 'package:editor/components/components.dart';
import 'package:editor/components/game_entity.dart';
import 'package:editor/utilities/undo_redo.dart';
import 'package:flutter/material.dart';
import 'package:xml/xml.dart' as xml;

class Script extends Component {
  late String name;

  Script({required this.name});

  @override
  Script.fromXml(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final xml.XmlElement? nameNode = (root.getElement("Name"));

    final String name = nameNode?.innerText ?? "UnnamedScript";

    Script(name: name);
  }

  @override
  String toXML() {
    final builder = xml.XmlBuilder();

    builder.element("Name", nest: () {
      builder.element(name);
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }

  @override
  MSComponent<Script> getMultiselectComponent(MSGameEntity msEntity) =>
      MSScript(msEntity);
}

enum ScriptProperty { name }

class MSScript extends MSComponent<Script> {
  final ValueNotifier<String> name = ValueNotifier<String>("");

  late RelayCommand updateComponentsCommand;

  MSScript(super.msEntity) {
    _createCommands();
    refresh();
    _initializeListeners();
  }

  @override
  bool updateComponents(propertyName) {
    if (propertyName is! ScriptProperty) return false;

    final updateMap = {ScriptProperty.name: (Script c) => c.name = name.value};

    for (var c in selectedComponents) {
      updateMap[propertyName]?.call(c);
    }

    return true;
  }

  @override
  bool updateMSComponent() {
    name.value =
        MSGameEntity.getMixedValue(selectedComponents, ((x) => x.name)) ?? "";

    return true;
  }

  void _initializeListeners() {
    name.addListener(
        () => updateComponentsCommand.execute(ScriptProperty.name));
  }

  void _createCommands() {
    updateComponentsCommand = RelayCommand<ScriptProperty>((x) {
      final List<dynamic> oldValues;
      final dynamic newValue;

      switch (x) {
        case ScriptProperty.name:
          newValue = name.value;
          oldValues = selectedComponents.map((script) => script.name).toList();
          break;
      }

      updateComponents(x);

      final Map<ScriptProperty, void Function(int, dynamic)> actions = {
        ScriptProperty.name: (i, other) => selectedComponents[i].name = other,
      };

      UndoRedo().add(UndoRedoAction(
        name: "",
        undoAction: () {
          for (int i = 0; i < oldValues.length; i++) {
            actions[x]?.call(i, oldValues[i]);
          }
          MSGameEntity.getMSGameEntity()?.refresh();
        },
        redoAction: () {
          for (int i = 0; i < oldValues.length; i++) {
            actions[x]?.call(i, newValue);
          }
          MSGameEntity.getMSGameEntity()?.refresh();
        },
      ));
    });
  }
}
