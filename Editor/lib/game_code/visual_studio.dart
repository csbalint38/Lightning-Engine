import 'dart:ffi';
import 'package:ffi/ffi.dart';

typedef _OpenVisualStudioNative = Void Function(Pointer<Utf8> solutionPath);
typedef _OpenVisualStudio = void Function(Pointer<Utf8> solutionPath);

typedef _CloseVisualStudioNative = Void Function();
typedef _CloseVisualStudio = void Function();

class VisualStudio {
  // TODO: replace
  static final _lib = DynamicLibrary.open(
      "C:/Users/balin/Documents/Lightning-Engine/VisualStudio/bin/x64/Debug/net8.0-windows10.0.22621.0/win-x64/native/vsi.dll");
  static final _OpenVisualStudio _open_vs_internal =
      _lib.lookupFunction<_OpenVisualStudioNative, _OpenVisualStudio>(
          'OpenVisualStudio');
  static final _CloseVisualStudio _close_vs_internal =
      _lib.lookupFunction<_CloseVisualStudioNative, _CloseVisualStudio>(
          'CloseVisualStudio');

  static void openVisualStudio(String solutionPath) {
    final solutionPathPointer = solutionPath.toNativeUtf8();

    _open_vs_internal(solutionPathPointer);

    malloc.free(solutionPathPointer);
  }

  static void closeVisualStudio() {
    _close_vs_internal();
  }
}
