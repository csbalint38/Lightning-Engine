import 'package:editor/Components/components.dart';
import 'package:editor/Components/transform.dart' as lng;
import 'package:flutter/material.dart';
import 'package:xml/xml.dart' as xml;
import '../utilities/math.dart';

class GameEntity {
  final ValueNotifier<String> name = ValueNotifier<String>("");
  final ValueNotifier<bool> isEnabled = ValueNotifier<bool>(false);
  final ValueNotifier<List<Component>> components =
      ValueNotifier<List<Component>>([]);

  GameEntity(List<Component>? components, {required name, isEnabled = true}) {
    this.name.value = name;
    this.isEnabled.value = isEnabled;
    if (components == null ||
        !components.any((element) => element is lng.Transform)) {
      components?.add(lng.Transform());
    }
    this.components.value.addAll(components!);
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

enum GameEntityProperty { name, isEnabled, component }

class MSGameEntity {
  final ValueNotifier<String> name = ValueNotifier<String>("");
  final ValueNotifier<bool?> isEnabled = ValueNotifier<bool?>(null);
  final ValueNotifier<List<IMSComponent>> components =
      ValueNotifier<List<IMSComponent>>([]);
  final List<GameEntity> selectedEntities;

  bool _enableUpdates = true;

  MSGameEntity(this.selectedEntities) {
    name.addListener(() =>
        _enableUpdates ? updateGameEntities(GameEntityProperty.name) : null);
    isEnabled.addListener(() => _enableUpdates
        ? updateGameEntities(GameEntityProperty.isEnabled)
        : null);
    components.addListener(() => _enableUpdates
        ? updateGameEntities(GameEntityProperty.component)
        : null);

    refresh();
  }

  static T? getMixedValue<T>(
      List<GameEntity> entities, T Function(GameEntity) getProperty) {
    T value = getProperty(entities.first);

    if (value is double) {
      for (final GameEntity entity in entities) {
        if (!Math.isNearEqual(value, getProperty(entity) as double?)) {
          return null;
        }
      }
    } else {
      for (final GameEntity entity in entities) {
        if (value != getProperty(entity)) {
          return null;
        }
      }
    }

    return value;
  }

  bool updateGameEntities(GameEntityProperty prop) {
    switch (prop) {
      case GameEntityProperty.name:
        for (final GameEntity entity in selectedEntities) {
          entity.name.value = name.value;
        }
        return true;
      case GameEntityProperty.isEnabled:
        for (final GameEntity entity in selectedEntities) {
          entity.isEnabled.value = isEnabled.value!;
        }
        return true;
      case GameEntityProperty.component:
        return false;
    }
  }

  void refresh() {
    _enableUpdates = false;
    name.value =
        getMixedValue<String>(selectedEntities, ((x) => x.name.value)) ?? "";
    isEnabled.value =
        getMixedValue<bool>(selectedEntities, ((x) => x.isEnabled.value));
    _enableUpdates = true;
  }
}
