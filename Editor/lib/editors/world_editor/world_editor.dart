import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/editors/world_editor/components.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:editor/editors/world_editor/history.dart';
import 'package:editor/editors/world_editor/icons.dart';
import 'package:editor/editors/world_editor/scens.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:docking/docking.dart';
import 'package:flutter/services.dart';

class WorldEditor extends StatefulWidget {
  final Project project;
  const WorldEditor({super.key, required this.project});

  @override
  State<WorldEditor> createState() => _WorldEditorState();
}

class _WorldEditorState extends State<WorldEditor> implements EventObserver {
  late final Project project;
  final _controller = WorldEditorController();

  @override
  void initState() {
    super.initState();
    project = widget.project;
    WorldEditorController().setProject(project);
    _controller.subscribe(this);
  }

  @override
  void dispose() {
    _controller.unsubscribe(this);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Shortcuts(
        shortcuts: <LogicalKeySet, Intent>{
          LogicalKeySet(
            LogicalKeyboardKey.controlLeft,
            LogicalKeyboardKey.keyZ,
          ): const UndoIntent(),
          LogicalKeySet(
            LogicalKeyboardKey.controlLeft,
            LogicalKeyboardKey.keyY,
          ): const RedoIntent(),
          LogicalKeySet(
            LogicalKeyboardKey.controlLeft,
            LogicalKeyboardKey.keyS,
          ): const SaveIntent(),
        },
        child: Actions(
          actions: <Type, Action<Intent>>{
            UndoIntent: CallbackAction<UndoIntent>(
              onInvoke: (intent) => _undo(),
            ),
            RedoIntent: CallbackAction<RedoIntent>(
              onInvoke: (intent) => _redo(),
            ),
            SaveIntent: CallbackAction<SaveIntent>(
              onInvoke: (intent) => _save(),
            ),
          },
          child: Focus(
            autofocus: true,
            child: MultiSplitViewTheme(
              data: Theme.of(context).msvTheme,
              child: TabbedViewTheme(
                data: Theme.of(context).tvTheme,
                child: Docking(
                  layout: DockingLayout(
                    root: DockingColumn(
                      [
                        DockingItem(
                          name: "Icons",
                          widget: const IconsRow(),
                          size: 45,
                          maximizable: false,
                        ),
                        DockingRow(
                          [
                            DockingColumn(
                              [
                                DockingItem(
                                  name: "Render Window",
                                  widget: const Text("A"),
                                ),
                                DockingRow(
                                  size: 240,
                                  [
                                    DockingItem(
                                      size: 180,
                                      name: "File Explorer",
                                      widget: const Text("B1"),
                                    ),
                                    DockingTabs(
                                      [
                                        DockingItem(
                                          name: "Content Browser",
                                          widget: const Text("B2.1"),
                                        ),
                                        DockingItem(
                                          name: "History",
                                          widget: const History(),
                                        )
                                      ],
                                    )
                                  ],
                                ),
                              ],
                            ),
                            DockingColumn(
                              size: 350,
                              [
                                DockingItem(
                                  name: "Game Entities",
                                  widget: const ScenesList(),
                                ),
                                DockingItem(
                                  name: "Components",
                                  widget: const Components(),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  void _undo() {
    _controller.undoCommand.execute(null);
  }

  void _redo() {
    _controller.redoCommand.execute(null);
  }

  void _save() {
    _controller.saveCommand.execute(null);
  }

  @override
  void notify(ViewEvent event) {}
}

class UndoIntent extends Intent {
  const UndoIntent();
}

class RedoIntent extends Intent {
  const RedoIntent();
}

class SaveIntent extends Intent {
  const SaveIntent();
}
