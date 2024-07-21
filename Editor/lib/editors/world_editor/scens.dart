import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';

class ScenesList extends StatefulWidget {
  const ScenesList({super.key});

  @override
  State<ScenesList> createState() => _ScenesListState();
}

class _ScenesListState extends State<ScenesList> implements EventObserver {
  final _controller = WorldEditorController();

  @override
  void initState() {
    super.initState();
    _controller.subscribe(this);
  }

  @override
  void dispose() {
    _controller.unsubscribe(this);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(8),
      child: Column(
        children: [
          Align(
            alignment: Alignment.centerLeft,
            child: ElevatedButton(
              onPressed: _controller.addScene,
              style: Theme.of(context).smallButton,
              child: const Text("Add scene"),
            ),
          ),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.only(top: 8),
              child: ListView.builder(
                itemCount: _controller.getScenes().length,
                itemBuilder: (context, index) {
                  return ExpansionTile(
                    controlAffinity: ListTileControlAffinity.leading,
                    tilePadding: EdgeInsets.zero,
                    minTileHeight: 0,
                    dense: true,
                    initiallyExpanded: _controller.getScenes()[index].isActive,
                    iconColor: Colors.blueGrey,
                    collapsedIconColor: Colors.blueGrey,
                    title: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          _controller.getScenes()[index].name,
                          style: Theme.of(context).accentSmall,
                        ),
                        OutlinedButton(
                          style: Theme.of(context).smallButton,
                          onPressed: () => _controller.removeScene(index),
                          child: const Text("Remove"),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  void notify(ViewEvent event) {
    if (event is ScenesListChanged) {
      setState(() {});
    }
  }
}
