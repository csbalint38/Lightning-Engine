import 'dart:typed_data';
import 'package:editor/components/game_entity.dart';

abstract class Component {
  Component();

  factory Component.fromXML(String xmlStr) {
    throw UnimplementedError();
  }

  String toXML() {
    throw UnimplementedError();
  }

  MSComponent getMultiselectComponent(MSGameEntity msEntity);
  void writeToBinary(BytesBuilder builder);
}

abstract class MSComponent<T extends Component> {
  late final List<T> selectedComponents;
  bool enableUpdates = true;
  late Enum properties;

  bool updateComponents(dynamic propertyName);
  bool updateMSComponent();

  void refresh() {
    enableUpdates = false;
    updateMSComponent();
    enableUpdates = true;
  }

  MSComponent(MSGameEntity msEntity) {
    selectedComponents = msEntity.selectedEntities
        .where((entity) => entity.getComponent<T>() != null)
        .map((entity) => entity.getComponent<T>()!)
        .toList();
  }
}
