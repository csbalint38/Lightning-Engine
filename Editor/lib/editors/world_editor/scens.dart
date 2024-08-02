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
  final _pageBucket = PageStorageBucket();
  final _controller = WorldEditorController();

  int selectedEntityIndex = 0;

  void _selectedEntityChanged(int index) {
    setState(() {
      selectedEntityIndex = index;
      _controller.notify(SelectedEntityIndexChanged(index: index));
    });
  }

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
    return Column(
      children: [
        Align(
          alignment: Alignment.centerLeft,
          child: Padding(
            padding: const EdgeInsets.all(8),
            child: ElevatedButton(
              onPressed: _controller.addScene,
              style: Theme.of(context).smallButton,
              child: const Text("Add scene"),
            ),
          ),
        ),
        Expanded(
          child: Padding(
            padding: const EdgeInsets.only(top: 8),
            child: PageStorage(
              bucket: _pageBucket,
              child: Material(
                child: ListView.builder(
                  itemCount: _controller.getScenes().length,
                  itemBuilder: (context, index) {
                    return ListTileTheme(
                      minVerticalPadding: 0,
                      child: ExpansionTile(
                        controlAffinity: ListTileControlAffinity.leading,
                        minTileHeight: 0,
                        initiallyExpanded:
                            _controller.getScenes()[index].isActive,
                        iconColor: Colors.blueGrey,
                        collapsedIconColor: Colors.blueGrey,
                        title: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              _controller.getScenes()[index].name,
                              style: Theme.of(context).accentSmall,
                            ),
                            Row(
                              children: [
                                Tooltip(
                                  message: "Add new GameEntity",
                                  child: OutlinedButton(
                                    style: Theme.of(context).smallIcon,
                                    onPressed: _controller
                                            .getScenes()[index]
                                            .isActive
                                        ? () => _controller.addGameEntity(index)
                                        : null,
                                    child: const Icon(
                                        Icons.add_circle_outline_sharp),
                                  ),
                                ),
                                Padding(
                                  padding: const EdgeInsets.only(right: 8),
                                  child: OutlinedButton(
                                    style: Theme.of(context).smallButton,
                                    onPressed: () =>
                                        _controller.removeScene(index),
                                    child: const Text("Remove"),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                        children: _controller
                            .getScenes()[index]
                            .entities
                            .asMap()
                            .entries
                            .map((entity) => GestureDetector(
                                  onTapUp: (_) =>
                                      _selectedEntityChanged(entity.key),
                                  child: ListTile(
                                    selectedTileColor:
                                        Theme.of(context).outlineColor,
                                    selectedColor:
                                        Theme.of(context).primaryColor,
                                    minTileHeight: 0,
                                    title: MouseRegion(
                                      cursor: SystemMouseCursors.click,
                                      child: Row(
                                        mainAxisAlignment:
                                            MainAxisAlignment.spaceBetween,
                                        children: [
                                          Text(
                                            entity.value.name,
                                            style: Theme.of(context).smallText,
                                          ),
                                          OutlinedButton(
                                            onPressed: _controller
                                                    .getScenes()[index]
                                                    .isActive
                                                ? () => _controller
                                                    .removeGameEntity(
                                                        index, entity.value)
                                                : null,
                                            style: Theme.of(context).smallIcon,
                                            child: const Icon(
                                                Icons.cancel_outlined),
                                          )
                                        ],
                                      ),
                                    ),
                                    titleTextStyle:
                                        Theme.of(context).accentSmall,
                                    selected: entity.key == selectedEntityIndex,
                                  ),
                                ))
                            .toList(),
                      ),
                    );
                  },
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }

  @override
  void notify(ViewEvent event) {
    if (event is ScenesListChanged) {
      setState(() {});
    }
    if (event is ActiveSceneEntitiesListChanged) {
      setState(() {});
    }
  }
}
