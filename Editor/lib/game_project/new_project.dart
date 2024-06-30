import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';

class NewProject extends StatefulWidget {
  const NewProject({super.key});

  @override
  State<NewProject> createState() => _NewProjectState();
}

class _NewProjectState extends State<NewProject> {
  static List<String> items = [
    "Template 1",
    "Template 2",
    "Template 3",
    "Template 4",
    "Template 5",
    "Template 6",
    "Template 7",
    "Template 8"
  ];
  final FocusNode _listFocus = FocusNode();

  int selectedThemeIndex = 0;

  void _selectedThemeChanged(int index) {
    setState(() {
      selectedThemeIndex = index;
    });
  }

  @override
  void dispose() {
    _listFocus.dispose();
    super.dispose();
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
                          itemCount: items.length,
                          itemBuilder: (BuildContext context, int index) {
                            return Listener(
                              onPointerUp: (_) => _selectedThemeChanged(index),
                              child: ListTile(
                                title: Text(
                                  items[index],
                                ),
                                selected: index == selectedThemeIndex,
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
                    child: Image.asset(
                      "ProjectTemplates/EmptyProject/screenshot.png",
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
                const Padding(
                  padding: EdgeInsets.fromLTRB(16, 16, 16, 4),
                  child: Row(
                    mainAxisAlignment: MainAxisAlignment.start,
                    children: [
                      SizedBox(
                        width: 60,
                        child: Text(
                          "Name",
                          style: TextStyle(
                            fontSize: 18,
                          ),
                        ),
                      ),
                      Expanded(
                        child: TextField(),
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
                      const Expanded(
                        child: TextField(),
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
                        onPressed: () {},
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
}
