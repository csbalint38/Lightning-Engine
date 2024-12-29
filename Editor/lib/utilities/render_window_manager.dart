import 'dart:ffi';
import 'dart:io';

import 'package:win32/win32.dart';

class RenderWindowManager {
  //TODO: Relapce with proper path
  static final String executable =
      r"C:\Users\balin\Documents\Lightning-Engine\x64\DebugEditor\RendererWindowManager.exe";
  static final String title = "RenderWindow";
  static int nextId = 1;
  static final Duration timeout = const Duration(seconds: 10);

  static String get lastWindowTitle => "$title${nextId - 1}";

  static Future<void> createWindow() async {
    await Process.start(
      executable,
      ["$title$nextId"],
      mode: ProcessStartMode.detached,
    );

    nextId++;
  }

  static Future<void> ensureCreated() async {
    final DateTime startTime = DateTime.now();

    while (DateTime.now().difference(startTime) < timeout) {
      if (FindWindow(TEXT(lastWindowTitle), nullptr) != 0) {
        return;
      }
      await Future.delayed(const Duration(milliseconds: 100));
    }
  }
}
