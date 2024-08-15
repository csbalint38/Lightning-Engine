import 'package:editor/Components/game_entity.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class Components extends StatefulWidget {
  final FocusNode focusNode;
  const Components({super.key, required this.focusNode});

  @override
  State<Components> createState() => _ComponentsState();
}

class _ComponentsState extends State<Components> {
  final _controller = WorldEditorController();
  final TextEditingController _nameController = TextEditingController();
  final FocusNode _localFocus = FocusNode();

  GameEntity? entity;

  @override
  void initState() {
    super.initState();
  }

  @override
  void dispose() {
    _nameController.dispose();
    super.dispose();
  }

  setGameEntity(int index) {
    entity = _controller.getActiveScene.entities.value[index];
    _nameController.text = entity!.name.value;
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder(
      valueListenable: _controller.selectedEntityIndices,
      builder: (context, value, _) {
        if (value.isEmpty) {
          return const Column();
        }
        setGameEntity(value[0]);

        return SingleChildScrollView(
            child: Padding(
          padding: const EdgeInsets.all(8),
          child: Column(
            children: [
              Row(
                children: [
                  ElevatedButton(
                    onPressed: () {},
                    style: Theme.of(context).smallButton,
                    child: const Text("Add Component"),
                  ),
                ],
              ),
              const SizedBox(height: 4),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Flexible(
                    child: Container(
                      constraints: const BoxConstraints(maxWidth: 250),
                      child: Focus(
                        focusNode: _localFocus,
                        onKeyEvent: (node, event) {
                          if (event is KeyDownEvent &&
                              event.logicalKey == LogicalKeyboardKey.escape) {
                            _localFocus.unfocus();
                            widget.focusNode.requestFocus();
                            return KeyEventResult.handled;
                          }
                          return KeyEventResult.ignored;
                        },
                        child: TextField(
                          textAlignVertical: TextAlignVertical.center,
                          cursorHeight: 16,
                          maxLines: 1,
                          controller: _nameController,
                          style: Theme.of(context).smallText,
                          decoration: Theme.of(context).smallInput,
                          onSubmitted: (name) {
                            _controller.setEntityName(entity!, name);
                            widget.focusNode.requestFocus();
                          },
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  Column(
                    children: [
                      Row(
                        children: [
                          Text(
                            "Enabled: ",
                            style: TextStyle(
                                fontSize: 14,
                                color: Theme.of(context).lightColor),
                          ),
                          Checkbox(
                            value: entity?.isEnabled.value,
                            onChanged: (isEnabled) {
                              _controller.enabeEntity(entity!, isEnabled!);
                              setState(() {});
                            },
                          ),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
              Row(
                children: [
                  Text(entity!.components.toString()),
                ],
              )
            ],
          ),
        ));
      },
    );
  }
}
