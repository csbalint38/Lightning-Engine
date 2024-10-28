import 'dart:async';
import 'dart:ffi';
import 'dart:isolate';
import 'package:editor/common/constants.dart';
import 'package:editor/game_project/project.dart';
import 'package:editor/utilities/capitalize.dart';
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
typedef _BuildSolutionNativeType = Bool Function(
    Pointer<Utf16>, Pointer<Utf16>, Bool);
typedef _BuildSolutionType = bool Function(
    Pointer<Utf16>, Pointer<Utf16>, bool);

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
  static final _BuildSolutionType _buildSolution =
      _vsiDll.lookupFunction<_BuildSolutionNativeType, _BuildSolutionType>(
          'build_solution');

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
      calloc.free(normalizedSolutionPathPtr);

      return result;
    });
  }

  static Future<bool> buildSolution(
      Project project, BuildConfig config, bool showWindow) async {
    return await _runInIsolate<bool>(() {
      final String solutionName =
          project.solution.replaceAll('/', '\\').replaceAll(r'\', r'\\');
      final Pointer<Utf16> solutionNamePtr = solutionName.toNativeUtf16();
      final String configName = capitalize(config.toString().split('.').last);
      final Pointer<Utf16> configNamePointer = configName.toNativeUtf16();

      final bool result =
          _buildSolution(solutionNamePtr, configNamePointer, showWindow);

      calloc.free(solutionNamePtr);
      calloc.free(configNamePointer);

      return result;
    });
  }

  static Future<T> _runInIsolate<T>(FutureOr<T> Function() task) async {
    final ReceivePort port = ReceivePort();
    await Isolate.spawn(_isolateEntry, [task, port.sendPort]);
    return await port.first as T;
  }

  static void _isolateEntry<T>(List<dynamic> args) async {
    Function task = args[0];
    SendPort port = args[1];

    final T result = await task();
    port.send(result);
  }
}
