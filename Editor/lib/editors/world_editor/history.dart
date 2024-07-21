import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/editors/world_editor/controllers/world_editor_controller.dart';
import 'package:flutter/material.dart';

class History extends StatefulWidget {
  const History({super.key});

  @override
  State<History> createState() => _HistoryState();
}

class _HistoryState extends State<History> implements EventObserver {
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
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: ListView.builder(
        itemCount: _controller.undoRedo.undoList.length +
            _controller.undoRedo.redoList.length,
        itemBuilder: (context, index) {
          if (index < _controller.undoRedo.undoList.length) {
            return Text(_controller.undoRedo.undoList[index].name);
          }
          final int idx = index - _controller.undoRedo.undoList.length;
          return Text(
            _controller.undoRedo.redoList[idx].name,
            style: const TextStyle(color: Colors.black38),
          );
        },
      ),
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
