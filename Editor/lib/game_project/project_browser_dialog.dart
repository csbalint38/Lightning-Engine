import 'package:editor/game_project/open_project.dart';
import 'package:flutter/material.dart';

class ProjectBrowserDialog extends StatefulWidget {
  const ProjectBrowserDialog({super.key});

  @override
  State<ProjectBrowserDialog> createState() => _ProjectBrowserDialogState();
}

class _ProjectBrowserDialogState extends State<ProjectBrowserDialog> {
  @override
  Widget build(BuildContext context) {
    return const OpenProject();
  }
}
