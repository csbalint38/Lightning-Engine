import 'package:editor/common/mvvm/observer.dart';
import 'package:editor/editors/dummy_editor.dart';
import 'package:editor/game_project/controllers/new_project_controller.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';

class NewProject extends StatefulWidget {
  const NewProject({super.key});

  @override
  State<NewProject> createState() => _NewProjectState();
}

class _NewProjectState extends State<NewProject> implements EventObserver {
  final _controller = NewProjectController();

  late final TextEditingController _nameController;
  late final TextEditingController _pathController;

  final FocusNode _listFocus = FocusNode();

  int selectedTemplateIndex = 0;

  void _selectedThemeChanged(int index) {
    setState(() {
      selectedTemplateIndex = index;
    });
  }

  void _navigateToEditor() async {
    final Project project = await _controller
        .createProject(_controller.getTemplates()[selectedTemplateIndex]);
    if (mounted) {
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(builder: (context) => Editor(project: project)),
      );
    }
  }

  @override
  void initState() {
    super.initState();

    _nameController = TextEditingController(text: _controller.name);
    _pathController = TextEditingController(text: _controller.path);

    _controller.subscribe(this);
  }

  @override
  void dispose() {
    _listFocus.dispose();
    _nameController.dispose();
    _pathController.dispose();
    super.dispose();
    _controller.unsubscribe(this);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        children: [
          Expanded(
            flex: 4,
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  flex: 3,
                  child: Container(
                    padding: const EdgeInsets.fromLTRB(48, 16, 0, 16),
                    child: Container(
                      height: 300,
                      decoration: BoxDecoration(
                        border: Border.all(
                            color: Theme.of(context).borderColor, width: 1),
                      ),
                      child: Material(
                        child: ListView.builder(
                          itemCount: _controller.getTemplates().length,
                          itemBuilder: (BuildContext context, int index) {
                            return Listener(
                              onPointerUp: (_) => _selectedThemeChanged(index),
                              child: ListTile(
                                title: Row(
                                  children: [
                                    Image.file(
                                      _controller.getTemplates()[index].icon,
                                      width: 25,
                                      height: 25,
                                    ),
                                    const SizedBox(width: 15),
                                    Text(
                                      _controller
                                          .getTemplates()[index]
                                          .projectName,
                                    ),
                                  ],
                                ),
                                selected: index == selectedTemplateIndex,
                              ),
                            );
                          },
                        ),
                      ),
                    ),
                  ),
                ),
                Expanded(
                  flex: 4,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(0, 16, 40, 16),
                    child: Image.file(
                      _controller
                          .getTemplates()[selectedTemplateIndex]
                          .screenshot,
                      alignment: Alignment.centerRight,
                    ),
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            flex: 3,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 16, 16, 4),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.start,
                    children: [
                      const SizedBox(
                        width: 60,
                        child: Text(
                          "Name",
                          style: TextStyle(
                            fontSize: 18,
                          ),
                        ),
                      ),
                      Expanded(
                        child: TextField(
                          controller: _nameController,
                          onChanged: (text) {
                            _controller.setName(text);
                          },
                        ),
                      ),
                    ],
                  ),
                ),
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 4, 16, 0),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.start,
                    children: [
                      const SizedBox(
                        width: 60,
                        child: Text(
                          "Path",
                          style: TextStyle(
                            fontSize: 18,
                          ),
                        ),
                      ),
                      Expanded(
                        child: TextField(
                          controller: _pathController,
                          onChanged: (text) {
                            _controller.setPath(text);
                          },
                        ),
                      ),
                      Padding(
                        padding: const EdgeInsets.only(left: 8),
                        child: OutlinedButton(
                          onPressed: () {},
                          child: const Text("Browse"),
                        ),
                      )
                    ],
                  ),
                ),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Padding(
                        padding: const EdgeInsets.only(top: 16),
                        child: Text(
                          _controller.errorMessage,
                          style: const TextStyle(color: Colors.red),
                        )),
                  ],
                ),
                Expanded(
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      OutlinedButton(
                        onPressed: () {
                          Navigator.pop(context);
                        },
                        child: const Text("Back"),
                      ),
                      const SizedBox(width: 50),
                      ElevatedButton(
                        onPressed: () =>
                            _controller.isValid ? _navigateToEditor() : null,
                        child: const Text("Create"),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  @override
  void notify(ViewEvent event) {
    if (event is NameChanged) {
      setState(() {
        _nameController.text = event.name;
      });
    } else if (event is PathChanged) {
      setState(() {
        _pathController.text = event.path;
      });
    }
  }
}
