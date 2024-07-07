import 'package:editor/game_project/project.dart';
import 'package:flutter/material.dart';
import 'package:window_manager/window_manager.dart';

class Editor extends StatefulWidget {
  final Project project;
  const Editor({super.key, required this.project});

  @override
  State<Editor> createState() => _EditorState();
}

class _EditorState extends State<Editor> {
  @override
  void initState() {
    super.initState();
    _resizeWindow();
  }

  Future<void> _resizeWindow() async {
    await windowManager.setMaximizable(true);
    await windowManager.setMaximumSize(const Size(1920, 1080));
    await windowManager.setSize(const Size(1080, 720));
    await windowManager.maximize();
  }

  @override
  Widget build(BuildContext context) {
    return const Placeholder();
  }
}
