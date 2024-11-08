import 'package:editor/components/game_entity.dart';
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
        Row(
          children: [
            Text(
              "Name: ",
              style: MSGameEntity.getMSGameEntity()?.isEnabled.value == false
                  ? TextStyle(color: Theme.of(context).disabledColor)
                  : null,
            ),
            const SizedBox(width: 20),
            Text(
              component.name.value,
              style: MSGameEntity.getMSGameEntity()?.isEnabled.value == false
                  ? TextStyle(color: Theme.of(context).disabledColor)
                  : null,
            ),
          ],
        )
      ],
    );
  }
}
