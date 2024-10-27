import 'dart:ffi';
import 'package:ffi/ffi.dart';
import 'package:path/path.dart' as p;
import 'package:editor/congifg/config.dart';
import 'package:editor/components/transform.dart';
import 'package:editor/components/game_entity.dart';
import 'package:editor/utilities/id.dart';
import 'package:win32/win32.dart';

typedef _CreateGameEntityNativeType = IdType Function(
    Pointer<GameEntityDescriptor>);
typedef _CreateGameEntityType = int Function(Pointer<GameEntityDescriptor>);
typedef _RemoveGameEntityNativeType = Void Function(IdType);
typedef _RemoveGameEntityType = void Function(int);
typedef _LoadGameCodeDllNativeType = Uint32 Function(Pointer<Utf8>);
typedef _LoadGameCodeDllType = int Function(Pointer<Utf8>);
typedef _UnloadGameCodeDllNativeType = Uint32 Function();
typedef _UnloadGameCodeDllType = int Function();
typedef _GetScriptNamesNativeType = Pointer<SAFEARRAY> Function();
typedef _GetScriptNamesType = Pointer<SAFEARRAY> Function();
typedef _GetScriptCreatorNativeType = Pointer Function(Pointer<Utf8>);
typedef _GetScriptCreatorType = Pointer Function(Pointer<Utf8>);

final class TransformComponentDescriptor extends Struct {
  @Array(3)
  external Array<Float> position;

  @Array(3)
  external Array<Float> rotation;

  @Array(3)
  external Array<Float> scale;
}

final class ScriptComponentDescriptor extends Struct {
  external Pointer scriptCreator;
}

final class GameEntityDescriptor extends Struct {
  external TransformComponentDescriptor transform;
  external ScriptComponentDescriptor script;
}

class EngineAPI {
  static const String _dllName =
      "x64/DebugEditor/EngineDll.dll"; // TODO: replace with proper path
  static final String _dllPath = Config().read<String>(ConfigProps.enginePath)!;
  static final DynamicLibrary _engineDll =
      DynamicLibrary.open(p.join(_dllPath, _dllName));

  static final _CreateGameEntityType _createGameEntity = _engineDll
      .lookupFunction<_CreateGameEntityNativeType, _CreateGameEntityType>(
          'create_game_entity');
  static final _RemoveGameEntityType _removeGameEntity = _engineDll
      .lookupFunction<_RemoveGameEntityNativeType, _RemoveGameEntityType>(
          'remove_game_entity');
  static final _LoadGameCodeDllType _loadGameCodeDll = _engineDll
      .lookupFunction<_LoadGameCodeDllNativeType, _LoadGameCodeDllType>(
          'load_game_code_dll');
  static final _UnloadGameCodeDllType _unloadGameCodeDll = _engineDll
      .lookupFunction<_UnloadGameCodeDllNativeType, _UnloadGameCodeDllType>(
          'unload_game_code_dll');
  static final _GetScriptNamesType _getScriptNames =
      _engineDll.lookupFunction<_GetScriptNamesNativeType, _GetScriptNamesType>(
          'get_script_names');
  static final _GetScriptCreatorType _getScriptCreator = _engineDll
      .lookupFunction<_GetScriptCreatorNativeType, _GetScriptCreatorType>(
          'get_script_creator');

  static int createGameEntity(GameEntity entity) {
    Pointer<GameEntityDescriptor> desc = malloc<GameEntityDescriptor>();

    {
      Transform c = entity.getComponent<Transform>()!;
      desc.ref.transform.position[0] = c.position.x;
      desc.ref.transform.position[1] = c.position.y;
      desc.ref.transform.position[2] = c.position.z;

      desc.ref.transform.rotation[0] = c.rotation.x;
      desc.ref.transform.rotation[1] = c.rotation.y;
      desc.ref.transform.rotation[2] = c.rotation.z;

      desc.ref.transform.scale[0] = c.scale.x;
      desc.ref.transform.scale[1] = c.scale.y;
      desc.ref.transform.scale[2] = c.scale.z;
    } // Transform component

    {
      //Script c = entity.getComponent<Script>();
    }

    final int result = _createGameEntity(desc);

    free(desc);

    return result;
  }

  static void removeGameEntity(GameEntity entity) {
    _removeGameEntity(entity.entityId);
  }

  static bool loadGameCodeDll(String dllPath) {
    Pointer<Utf8> dllPathPtr = dllPath.toNativeUtf8();

    int result = _loadGameCodeDll(dllPathPtr);

    return result == 1;
  }

  static bool unloadGameCodeDll() => _unloadGameCodeDll() == 1;

  static List<String> getScriptNames() {
    final Pointer<SAFEARRAY> scriptNamesPtr = _getScriptNames();
    if (scriptNamesPtr == nullptr) {
      return [];
    }

    final List<String> scriptNames = [];
    final Pointer<Int32> pUbound = calloc<Int32>();
    final Pointer<Int32> pLbound = calloc<Int32>();

    try {
      SafeArrayGetLBound(scriptNamesPtr, 1, pLbound);
      SafeArrayGetUBound(scriptNamesPtr, 1, pUbound);

      final int count = pUbound.value - pLbound.value + 1;

      for (int i = 0; i < count; i++) {
        final itemPtr = calloc<VARIANT>();
        SafeArrayGetElement(
            scriptNamesPtr, calloc<Int32>()..value = i, itemPtr.cast());

        final bstrPtr = itemPtr.ref.bstrVal;

        scriptNames.add(bstrPtr.toDartString());

        free(itemPtr);
      }

      return scriptNames;
    } finally {
      free(pUbound);
      free(pLbound);
      SafeArrayDestroy(scriptNamesPtr);
    }
  }

  Pointer getScriptCreator(String name) {
    final Pointer<Utf8> namePointer = name.toNativeUtf8();
    final Pointer result = _getScriptCreator(namePointer);
    calloc.free(namePointer);

    return result;
  }

  EngineAPI._();
}
