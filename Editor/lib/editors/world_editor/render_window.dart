import 'dart:ffi';

import 'package:editor/utilities/render_window_manager.dart';
import 'package:flutter/material.dart';
import 'package:flutter_native_view/flutter_native_view.dart';
import 'package:win32/win32.dart';

class RenderWindow extends StatefulWidget {
  const RenderWindow({super.key});

  @override
  State<RenderWindow> createState() => _RenderWindowState();
}

class _RenderWindowState extends State<RenderWindow> {
  final controllers = List.generate(
    4,
    (index) => NativeViewController(
      handle: FindWindow(
        TEXT(RenderWindowManager.windowTitles[index]),
        nullptr,
      ),
      hitTestBehavior: HitTestBehavior.translucent,
    ),
  );

  @override
  void dispose() {
    for (var element in controllers) {
      element.dispose();
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final gridWidth = constraints.maxWidth / 2;
        final gridHeight = constraints.maxHeight / 2;

        return Column(
          children: [
            Row(
              children: [
                NativeView(
                  controller: controllers[0],
                  width: gridWidth,
                  height: gridHeight,
                ),
                NativeView(
                  controller: controllers[1],
                  width: gridWidth,
                  height: gridHeight,
                ),
              ],
            ),
            Row(
              children: [
                NativeView(
                  controller: controllers[2],
                  width: gridWidth,
                  height: gridHeight,
                ),
                NativeView(
                  controller: controllers[3],
                  width: gridWidth,
                  height: gridHeight,
                ),
              ],
            ),
          ],
        );
      },
    );
  }
}
