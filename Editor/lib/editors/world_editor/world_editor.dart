import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:editor/editors/world_editor/history.dart';
import 'package:editor/editors/world_editor/icons.dart';
import 'package:editor/editors/world_editor/scens.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:docking/docking.dart';

class WorldEdotor extends StatefulWidget {
  final Project project;
  const WorldEdotor({super.key, required this.project});

  @override
  State<WorldEdotor> createState() => _WorldEdotorState();
}

class _WorldEdotorState extends State<WorldEdotor> {
  late final Project project;

  @override
  void initState() {
    super.initState();
    project = widget.project;
    WorldEditorController().setProject(project);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: MultiSplitViewTheme(
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
                            widget: const Text("D"),
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
    );
  }
}
