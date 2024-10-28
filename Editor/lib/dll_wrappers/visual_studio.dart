import 'dart:async';
import 'dart:ffi';
import 'dart:isolate';
import 'package:editor/common/constants.dart';
import 'package:editor/game_project/project.dart';
import 'package:ffi/ffi.dart';
import 'package:path/path.dart' as p;
import 'package:editor/congifg/config.dart';

typedef _OpenVisualStudioNativeType = Void Function(Pointer<Utf16>);
typedef _OpenVisualStudioType = void Function(Pointer<Utf16>);
typedef _CloseVisualStudioNativeType = Void Function();
typedef _CloseVisualStudioType = void Function();
typedef _AddFilesNativeType = Bool Function(
    Pointer<Utf16>, Pointer<Utf16>, Pointer<Pointer<Utf16>>, Int32);
typedef _AddFilesType = bool Function(
    Pointer<Utf16>, Pointer<Utf16>, Pointer<Pointer<Utf16>>, int);

final class VisualStudio {
  static const String _dllName =
      "x64/DebugEditor/vsidll.dll"; // TODO: replace with proper path
  static final String _dllPath = Config().read<String>(ConfigProps.enginePath)!;
  static final DynamicLibrary _vsiDll =
      DynamicLibrary.open(p.join(_dllPath, _dllName));

  static final _OpenVisualStudioType _openVisualStudio = _vsiDll.lookupFunction<
      _OpenVisualStudioNativeType, _OpenVisualStudioType>('open_visual_studio');
  static final _CloseVisualStudioType _closeVisualStudio = _vsiDll
      .lookupFunction<_CloseVisualStudioNativeType, _CloseVisualStudioType>(
          'close_visual_studio');
  static final _AddFilesType _addFiles =
      _vsiDll.lookupFunction<_AddFilesNativeType, _AddFilesType>('add_files');

  static bool isDebugging = false;
  static bool buildDone = true;
  static bool buildSucceeded = false;

  static Future<void> openVisualStudio(String solutionPath) async {
    await _runInIsolate(() {
      final Pointer<Utf16> solutionPathPtr = solutionPath.toNativeUtf16();
      _openVisualStudio(solutionPathPtr);
      calloc.free(solutionPathPtr);
    });
  }

  static Future<void> closeVisualStudio() async {
    await _runInIsolate(() {
      _closeVisualStudio();
    });
  }

  static Future<bool> addFiles(String solutionPath, List<String> files) async {
    return await _runInIsolate<bool>(() {
      final String normalizedSolutionPath =
          solutionPath.replaceAll('/', '\\').replaceAll(r'\', r'\\');
      final Pointer<Utf16> normalizedSolutionPathPtr =
          normalizedSolutionPath.toNativeUtf16();
      final Pointer<Utf16> projectPtr = normalizedSolutionPath
          .split('\\')
          .last
          .split('.')
          .first
          .toNativeUtf16();

      final Pointer<Pointer<Utf16>> filesPointers =
          calloc<Pointer<Utf16>>(files.length);

      for (int i = 0; i < files.length; i++) {
        filesPointers[i] = files[i].toNativeUtf16();
      }

      final result = _addFiles(
          normalizedSolutionPathPtr, projectPtr, filesPointers, files.length);

      calloc.free(normalizedSolutionPathPtr);
      calloc.free(projectPtr);

      for (int i = 0; i < files.length; i++) {
        calloc.free(filesPointers[i]);
      }

      calloc.free(filesPointers);

      return result;
    });
  }

  static Future<bool> buildSolution(
      Project project, BuildConfig config, bool show_window) async {
        isDebugging = true;
        buildDone = false;
        // call the API
        buildSucceeded = true; // api result
        buildDone = true;
        isDebugging = false;

        return buildSucceeded;
      }

  static Future<void> run(Project project, BuildConfig config, bool debug) async {
    isDebugging = true;
  }

  static Future<void> stop() async {
    isDebugging = false;
  }

  static Future<T> _runInIsolate<T>(FutureOr<T> Function() task) async {
    final ReceivePort port = ReceivePort();
    await Isolate.spawn(_isolateEntry, [task, port.sendPort]);
    return await await port.first as T;
  }

  static void _isolateEntry<T>(List<dynamic> args) async {
    Function task = args[0];
    SendPort port = args[1];

    final T result = await task();
    port.send(result);
  }
}
