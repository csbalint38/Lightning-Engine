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
          message: "Undo\nCtrl + Z",
          child: IconButton(
            onPressed: _controller.undoRedo.undoList.isNotEmpty
                ? () => _controller.undoCommand.execute(null)
                : null,
            icon: const Icon(Icons.undo),
          ),
        ),
        Tooltip(
          message: "Redo\nCtrl + Y",
          child: IconButton(
            onPressed: _controller.undoRedo.redoList.isNotEmpty
                ? () => _controller.redoCommand.execute(null)
                : null,
            icon: const Icon(Icons.redo),
          ),
        ),
        Tooltip(
          message: "Save\nCtrl + S",
          child: IconButton(
            onPressed: _controller.saveCommand.canExecute(null)
                ? () => _controller.saveCommand.execute(null)
                : null,
            icon: const Icon(Icons.save),
          ),
        )
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
    if (event is ProjectSaved) {
      setState(() {});
    }
  }
}
