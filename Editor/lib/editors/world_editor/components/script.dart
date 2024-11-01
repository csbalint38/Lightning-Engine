import 'package:editor/components/script.dart';
import 'package:editor/editors/world_editor/components/base_component.dart';
import 'package:flutter/material.dart';

class Script extends StatelessWidget {
  final MSScript component;
  final FocusNode globalFocus;

  const Script({required this.globalFocus, required this.component, super.key});

  @override
  Widget build(BuildContext context) {
    return BaseComponent(
      title: "Script",
      children: [
        ValueListenableBuilder(
          valueListenable: component.name,
          builder: (context, value, child) => Text(value),
        ),
      ],
    );
  }
}
