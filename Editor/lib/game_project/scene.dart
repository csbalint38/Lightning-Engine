import 'package:editor/Components/game_entity.dart';
import 'package:editor/common/mvvm/observable_list.dart';
import 'package:editor/common/relay_command.dart';
import 'package:editor/utilities/undo_redo.dart';
import 'package:xml/xml.dart' as xml;

class Scene {
  static final UndoRedo _undoRedo = UndoRedo();

  String name;
  bool isActive;
  final ObservableList<GameEntity> entities = ObservableList<GameEntity>();

  late RelayCommand addGameEntity;
  late RelayCommand removeGameEntity;

  Scene({
    required this.name,
    this.isActive = false,
    List<GameEntity> entities = const <GameEntity>[],
  }) {
    this.entities.addAll(entities);

    addGameEntity = RelayCommand<GameEntity>(
      (x) {
        _addGameEntity(x);
        int index = this.entities.length - 1;
        _undoRedo.add(
          UndoRedoAction(
            name: "Add ${x.name} to $name",
            undoAction: () => _removeGameEntity(x),
            redoAction: () => this.entities.insert(index, x),
          ),
        );
      },
    );

    removeGameEntity = RelayCommand<GameEntity>(
      (x) {
        int index = this.entities.indexOf(x);
        _removeGameEntity(x);
        _undoRedo.add(
          UndoRedoAction(
            name: "Remove ${x.name}",
            undoAction: () => this.entities.insert(index, x),
            redoAction: () => _removeGameEntity(x),
          ),
        );
      },
    );
  }

  factory Scene.fromXML(String xmlStr) {
    xml.XmlDocument document = xml.XmlDocument.parse(xmlStr);

    xml.XmlElement root = document.rootElement;
    final String name = (root.getElement("Name")?.innerText)!;
    final bool isActive =
        (((root.getElement("IsActive")?.innerText)!).toLowerCase() == 'true');
    final xml.XmlElement? entitiesNode = (root.getElement("Entities"));
    final List<GameEntity> entities = <GameEntity>[];

    if (entitiesNode != null) {
      for (final entity in entitiesNode.findElements("GameEntity")) {
        entities.add(GameEntity.fromXML(entity.toString()));
      }
    }

    return Scene(name: name, isActive: isActive, entities: entities);
  }

  String toXML() {
    final builder = xml.XmlBuilder();
    builder.element("Scene", nest: () {
      builder.element("Name", nest: name);
      builder.element("IsActive", nest: isActive.toString());
      builder.element("Entities", nest: () {
        for (final entity in entities) {
          builder.xml(entity.toXML());
        }
      });
    });

    return builder.buildDocument().toXmlString(pretty: true);
  }

  _addGameEntity(GameEntity entity) {
    entities.add(entity);
  }

  _removeGameEntity(GameEntity entity) {
    entities.remove(entity);
  }
}
