import 'package:editor/Components/game_entity.dart';
import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:flutter/material.dart';

class Components extends StatefulWidget {
  const Components({super.key});

  @override
  State<Components> createState() => _ComponentsState();
}

class _ComponentsState extends State<Components> implements EventObserver {
  final _controller = WorldEditorController();

  late GameEntity entity;

  @override
  void initState() {
    _controller.subscribe(this);
    entity = _controller.getActiveScene.entities[0];
    super.initState();
  }

  @override
  void dispose() {
    _controller.unsubscribe(this);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: Column(
        children: [
          Row(
            children: [
              Text(entity.name),
            ],
          ),
          Row(
            children: [
              Text(entity.components.toString()),
            ],
          )
        ],
      ),
    );
  }

  @override
  void notify(ViewEvent event) {
    if (event is SelectedEntityIndexChanged) {
      setState(() {
        entity = _controller.getActiveScene.entities[event.index];
      });
    }
  }
}
