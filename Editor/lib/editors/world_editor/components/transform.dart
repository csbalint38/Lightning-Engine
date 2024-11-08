import 'package:editor/components/game_entity.dart';
import 'package:editor/components/transform.dart';
import 'package:editor/common/vector_box.dart';
import 'package:editor/editors/world_editor/components/base_component.dart';
import 'package:flutter/material.dart';

class Transform extends StatelessWidget {
  final MSTransform component;
  final FocusNode globalFocus;

  const Transform(
      {required this.globalFocus, required this.component, super.key});

  @override
  Widget build(BuildContext context) {
    return BaseComponent(
      title: "Transform",
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            SizedBox(
              width: 60,
              child: Text(
                "Position:",
                style: MSGameEntity.getMSGameEntity()?.isEnabled.value == false
                    ? TextStyle(color: Theme.of(context).disabledColor)
                    : null,
              ),
            ),
            VectorBox(
              globalFocus,
              component.position,
              isEnabled:
                  MSGameEntity.getMSGameEntity()?.isEnabled.value == true,
            ),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            SizedBox(
              width: 60,
              child: Text(
                "Rotation:",
                style: MSGameEntity.getMSGameEntity()?.isEnabled.value == false
                    ? TextStyle(color: Theme.of(context).disabledColor)
                    : null,
              ),
            ),
            VectorBox(
              globalFocus,
              component.rotation,
              isEnabled:
                  MSGameEntity.getMSGameEntity()?.isEnabled.value == true,
            ),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            SizedBox(
              width: 60,
              child: Text(
                "Scale:",
                style: MSGameEntity.getMSGameEntity()?.isEnabled.value == false
                    ? TextStyle(color: Theme.of(context).disabledColor)
                    : null,
              ),
            ),
            VectorBox(
              globalFocus,
              component.scale,
              isEnabled:
                  MSGameEntity.getMSGameEntity()?.isEnabled.value == true,
            ),
          ],
        ),
      ],
    );
  }
}
