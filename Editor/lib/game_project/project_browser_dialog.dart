import 'package:flutter/material.dart';

class ProjectBrowserDialog extends StatefulWidget {
  const ProjectBrowserDialog({super.key});

  @override
  State<ProjectBrowserDialog> createState() => _ProjectBrowserDialogState();
}

class _ProjectBrowserDialogState extends State<ProjectBrowserDialog> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
          title: Text(
            "Lightning Editor",
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          backgroundColor: Theme.of(context).primaryColor),
      body: ElevatedButton(
        onPressed: () async {},
        child: const Text("Open Project"),
      ),
    );
  }
}
