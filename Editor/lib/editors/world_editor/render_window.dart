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
  final controller = NativeViewController(
    handle: FindWindow(TEXT(RenderWindowManager.lastWindowTitle), nullptr),
    hitTestBehavior: HitTestBehavior.translucent,
  );

  @override
  void dispose() {
    controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        return NativeView(
          controller: controller,
          width: constraints.maxWidth,
          height: constraints.maxHeight,
        );
      },
    );
  }
}
