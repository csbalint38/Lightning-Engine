import 'dart:io';

import 'package:editor/common/constants.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/utilities/logger.dart';
import 'package:flutter/widgets.dart';
import 'package:path/path.dart' as p;

class NewScriptController {
  final Project project;
  ValueNotifier<String> errorMessage = ValueNotifier<String>("");
  ValueNotifier<String> name = ValueNotifier<String>("NewScript");
  ValueNotifier<String> path = ValueNotifier<String>("");

  NewScriptController(this.project) {
    path.value = p.join(project.path, 'Code');
  }

  String get _namespace => project.name.replaceAll(' ', '_').toLowerCase();

  bool validate() {
    errorMessage.value = "";

    if(name.value.isEmpty) {
      errorMessage.value = "Type in a script name";
      return false;
    }

    if(fileInvalidCharacters.hasMatch(name.value) || name.value.contains(' ')) {
      errorMessage.value = "Invalid character(s) used in script name";
      return false;
    }

    if(path.value.isEmpty) {
      errorMessage.value = "Select script folder";
      return false;
    }

    if(!pathAllowedCharacters.hasMatch(path.value)) {
      errorMessage.value = "Invalid character(s) used in script path";
      return false;
    }

    if(path.value.startsWith(p.join(project.path, "Code"))) {
      errorMessage.value = "Script must be added to Code folder or to its subfolder";
      return false;
    }

    if(File(p.join(path.value, "${name.value}.cpp")).existsSync() || File(p.join(path.value, "${name.value}.h")).existsSync()) {
      errorMessage.value = "Script ${name.value} alredy exists in this folder";
      return false;
    }

    return true;
  }

  Future<void> create() async {
    if(!validate()) return;

    try {
      // await Future.delayed(Duration(seconds: 10));
      await Future(() async {
        if(!Directory(path.value).existsSync()) {
          Directory(path.value).create(recursive: true);
        }

        String h = p.join(path.value, "${name.value}.h");
        String cpp = p.join(path.value, "${name.value}.cpp");

        String hTemplate = await File(p.join('..', 'templates', 'h.txt')).readAsString();
        hTemplate = hTemplate.replaceAll('{{0}}', 'AAAA');
      });
    }
    catch(e) {
      debugLogger.e(e);
      EditorLogger().log(LogLevel.error, "Failed to create script ${name.value}", trace: StackTrace.current);
    }
  }
}