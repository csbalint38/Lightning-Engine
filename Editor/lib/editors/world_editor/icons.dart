import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:flutter/material.dart';

class IconsRow extends StatefulWidget {
  const IconsRow({super.key});

  @override
  State<IconsRow> createState() => _IconsRowState();
}

class _IconsRowState extends State<IconsRow> implements EventObserver {
  final _controller = WorldEditorController();

  @override
  void initState() {
    _controller.subscribe(this);
    super.initState();
  }

  @override
  void dispose() {
    _controller.unsubscribe(this);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        const SizedBox(width: 8),
        Tooltip(
          message: "Undo",
          child: IconButton(
            onPressed: _controller.undoRedo.undoList.isNotEmpty
                ? () => _controller.undo.execute(null)
                : null,
            icon: const Icon(Icons.undo),
          ),
        ),
        Tooltip(
          message: "Redo",
          child: IconButton(
            onPressed: _controller.undoRedo.redoList.isNotEmpty
                ? () => _controller.redo.execute(null)
                : null,
            icon: const Icon(Icons.redo),
          ),
        ),
      ],
    );
  }

  @override
  void notify(ViewEvent event) {
    if (event is UndoListChanged) {
      setState(() {});
    }
    if (event is RedoListChanged) {
      setState(() {});
    }
  }
}
