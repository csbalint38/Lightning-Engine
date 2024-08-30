import 'package:editor/game_code/controllers/new_script_controller.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';

class NewScriptDialog extends Dialog {
  late final NewScriptController _controller;
  late final TextEditingController _nameController;
  late final TextEditingController _pathController;

  NewScriptDialog(projectPath, {super.key}) {
    _controller = NewScriptController(projectPath);
    _nameController = TextEditingController(text: _controller.name.value);
    _pathController = TextEditingController(text: _controller.path.value);
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
            Padding(
              padding: const EdgeInsets.only(bottom: 20),
              child: Text("New Script", style: Theme.of(context).textTheme.headlineSmall,),
            ),
            Row(
              children: [
                Padding(
                  padding: const EdgeInsets.only(right: 8),
                  child: Text("Script name:"),
                ),
                SizedBox(
                  width: 480,
                  child: TextField(
                    textAlignVertical: TextAlignVertical.center,
                    cursorHeight: 16,
                    maxLines: 1,
                    style: Theme.of(context).smallText,
                    decoration: Theme.of(context).smallInput,
                    controller: _nameController,
                    onChanged: (value) => _controller.validate(),
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.only(right: 8),
                  child: Text("Path:"),
                ),
                SizedBox(
                  width: 480,
                  child: TextField(
                    textAlignVertical: TextAlignVertical.center,
                    cursorHeight: 16,
                    maxLines: 1,
                    style: Theme.of(context).smallText,
                    decoration: Theme.of(context).smallInput,
                    controller: _pathController,
                    onChanged: (value) =>
                      _controller.validate(),
                  ),
                ),
              ],
            ),
            ValueListenableBuilder(valueListenable: _controller.errorMessage, builder: (context, value, _) {
              return Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Padding(
                    padding: EdgeInsets.only(top: 8),
                    child: value.isNotEmpty ? Text(_controller.errorMessage.value, style: TextStyle(color: Colors.red),) : Text("${_controller.name.value}.h and ${_controller.name.value}.cpp will be added to ${_controller.path.value}")
                  ),
                ],
              );
            }),
            SizedBox(height: 20),
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
              TextButton(onPressed: () => Navigator.of(context).pop(), child: Text("Cancel")),
              TextButton(onPressed: () async {
                await _controller.create();
                Navigator.of(context).pop();
              }, child: Text("Create"),)
            ],),
          ],
          ),
        ),
      );
  }
}