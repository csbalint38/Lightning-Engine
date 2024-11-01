import 'package:editor/game_project/project_browser_dialog.dart';
import 'package:editor/themes/theme_manager.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:window_manager/window_manager.dart';

import 'package:editor/game_project/project_browser_dialog.dart';
import 'package:editor/themes/theme_manager.dart';
import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:window_manager/window_manager.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      home: Scaffold(
        appBar: AppBar(title: Text('Toggle Button with Nested Submenu')),
        body: Center(child: ToggleButtonWithNestedSubmenu()),
      ),
    );
  }
}

class ToggleButtonWithNestedSubmenu extends StatelessWidget {
  void _showSubMenu(BuildContext context, Offset position) async {
    // Create the main menu
    final String? selectedValue = await showMenu<String>(
      context: context,
      position: RelativeRect.fromLTRB(
          position.dx, position.dy, position.dx + 1, position.dy + 1),
      items: [
        PopupMenuItem<String>(
          value: 'Option 1',
          child: Text('Option 1'),
        ),
        PopupMenuItem<String>(
          value: 'Option 2',
          child: GestureDetector(
            onTap: () {
              // Show submenu and keep main menu open
              _showSubOptions(context, Offset(position.dx + 150, position.dy));
            },
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('Option 2'),
                Icon(Icons.arrow_right),
              ],
            ),
          ),
        ),
        PopupMenuItem<String>(
          value: 'Option 3',
          child: Text('Option 3'),
        ),
      ],
    );

    if (selectedValue != null) {
      print("Selected: $selectedValue");
    }
  }

  void _showSubOptions(BuildContext context, Offset position) async {
    final String? subSelectedValue = await showMenu<String>(
      context: context,
      position: RelativeRect.fromLTRB(
          position.dx, position.dy, position.dx + 1, position.dy + 1),
      items: [
        PopupMenuItem<String>(
          value: 'Sub Option 1',
          child: Text('Sub Option 1'),
        ),
        PopupMenuItem<String>(
          value: 'Sub Option 2',
          child: Text('Sub Option 2'),
        ),
      ],
    );

    if (subSelectedValue != null) {
      print("Selected Sub Option: $subSelectedValue");
    }
  }

  @override
  Widget build(BuildContext context) {
    return Builder(
      builder: (context) {
        return ElevatedButton.icon(
          icon: Icon(Icons.menu),
          label: Text("Toggle Menu"),
          onPressed: () async {
            final RenderBox button = context.findRenderObject() as RenderBox;
            final Offset position =
                button.localToGlobal(Offset(0, button.size.height));
            _showSubMenu(context, position);
          },
        );
      },
    );
  }
}





/*
void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await windowManager.ensureInitialized();

  WindowOptions windowOptions = const WindowOptions(
    size: Size(800, 600),
    center: true,
    title: "Lightning Engine",
    minimumSize: Size(800, 600),
    maximumSize: Size(800, 600),
  );

  windowManager.waitUntilReadyToShow(windowOptions, () async {
    await windowManager.show();
    await windowManager.focus();
    await windowManager.setMaximizable(false);
  });

  runApp(const LightningEditor());
}

ThemeManager _themeManager = ThemeManager();

class LightningEditor extends StatelessWidget {
  const LightningEditor({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: "Lightning Editor",
      theme: lightTheme,
      darkTheme: darkTheme,
      themeMode: _themeManager.themeMode,
      home: const ProjectBrowserDialog(),
    );
  }
}*/
