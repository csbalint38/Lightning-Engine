import 'package:editor/editors/world_editor/components.dart';
import 'package:editor/editors/world_editor/console.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:editor/editors/world_editor/history.dart';
import 'package:editor/editors/world_editor/icons.dart';
import 'package:editor/editors/world_editor/render_window.dart';
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

class _WorldEditorState extends State<WorldEditor> {
  late final Project project;
  final _controller = WorldEditorController();
  final FocusNode mainFocusNode = FocusNode();

  @override
  void initState() {
    super.initState();
    project = widget.project;
    WorldEditorController().setProject(project);
    setState(() {});
  }

  @override
  void dispose() {
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
          LogicalKeySet(
            LogicalKeyboardKey.f7,
          ): const BuildIntent(),
          LogicalKeySet(
            LogicalKeyboardKey.controlLeft,
            LogicalKeyboardKey.shiftLeft,
            LogicalKeyboardKey.keyB,
          ): const BuildIntent(),
          LogicalKeySet(
            LogicalKeyboardKey.controlLeft,
            LogicalKeyboardKey.f5,
          ): const StartWithoutDebuggingIntent(),
          LogicalKeySet(
            LogicalKeyboardKey.f5,
          ): const StartWithDebuggingIntent(),
          LogicalKeySet(
            LogicalKeyboardKey.shiftLeft,
            LogicalKeyboardKey.f5,
          ): const StopDebuggingIntent()
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
            BuildIntent: CallbackAction<BuildIntent>(
              onInvoke: (intent) => _build(),
            ),
            StartWithDebuggingIntent: CallbackAction<StartWithDebuggingIntent>(
              onInvoke: (intent) => _startWithDebugging(),
            ),
            StartWithoutDebuggingIntent:
                CallbackAction<StartWithoutDebuggingIntent>(
              onInvoke: (intent) => _startWithoutDebugging(),
            ),
            StopDebuggingIntent: CallbackAction<StopDebuggingIntent>(
              onInvoke: (intent) => _stopDebugging(),
            ),
          },
          child: Focus(
            autofocus: true,
            focusNode: mainFocusNode,
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
                                  widget: RenderWindow(),
                                  keepAlive: true,
                                  maximizable: false,
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
                                          name: "Console",
                                          widget: const Console(),
                                          keepAlive: true,
                                        ),
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
                              minimalSize: 150,
                              size: 350,
                              [
                                DockingItem(
                                  name: "Game Entities",
                                  widget: const ScenesList(),
                                ),
                                DockingItem(
                                  name: "Components",
                                  widget: Components(focusNode: mainFocusNode),
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

  void _build() {
    _controller.buildCommand.execute(null);
  }

  void _startWithDebugging() {
    _controller.debugStartCommand.execute(null);
  }

  void _startWithoutDebugging() {
    _controller.debugStartWithoutDebuggingCommand.execute(null);
  }

  void _stopDebugging() {
    _controller.debugStopCommand.execute(null);
  }
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

class BuildIntent extends Intent {
  const BuildIntent();
}

class StartWithDebuggingIntent extends Intent {
  const StartWithDebuggingIntent();
}

class StartWithoutDebuggingIntent extends Intent {
  const StartWithoutDebuggingIntent();
}

class StopDebuggingIntent extends Intent {
  const StopDebuggingIntent();
}
