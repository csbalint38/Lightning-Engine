import 'dart:ffi';
import 'dart:typed_data';
import 'package:editor/content/geometry.dart' as geometry;
import 'package:editor/utilities/capitalize.dart';
import 'package:editor/utilities/logger.dart';
import 'package:ffi/ffi.dart';
import 'package:path/path.dart' as p;
import 'package:editor/congifg/config.dart';
import 'package:win32/win32.dart';

typedef _CreatePrimitiveMeshNativeType = Void Function(Pointer<SceneData>, Pointer<PrimitiveInitInfo>);
typedef _CreatePrimitiveMeshType = void Function(Pointer<SceneData>, Pointer<PrimitiveInitInfo>);

final class GeometryImportSettings extends Struct {
  @Float()
  external double smoothingAngle;

  @Uint8()
  external int calculateNormals;

  @Uint8()
  external int calculateTangents;

  @Uint8()
  external int reverseHandedness;

  @Uint8()
  external int importEmbededTextures;

  @Uint8()
  external int importAnimations;
}

final class SceneData extends Struct {
  external Pointer<Uint8> data;

  @Uint32()
  external int dataSize;

  external GeometryImportSettings importSettings;
}

final class PrimitiveInitInfo extends Struct {
  @Uint32()
  external int type;

  @Array(3)
  external Array<Uint32> segments;

  @Array(3)
  external Array<Float> size;

  @Uint32()
  external int lod;
}

final class ContentToolsAPI {
  static const String _dllName =
      "x64/DebugEditor/ContentTools.dll"; // TODO: replace with proper path
  static final String _dllPath = Config().read<String>(ConfigProps.enginePath)!;
  static final DynamicLibrary _toolsDll = DynamicLibrary.open(p.join(_dllPath, _dllName));

  static final _CreatePrimitiveMeshType _createPrimitiveMesh = _toolsDll
      .lookupFunction<_CreatePrimitiveMeshNativeType, _CreatePrimitiveMeshType>(
          'create_primitive_mesh');

  static void createPrimitiveMesh(geometry.Geometry geometry, geometry.PrimitiveInitInfo info) {
    final Pointer<SceneData> sceneData = malloc<SceneData>();
    sceneData.ref.data = nullptr;
    sceneData.ref.dataSize = 0;
    sceneData.ref.importSettings.smoothingAngle = 0.0;
    sceneData.ref.importSettings.calculateNormals = 1;
    sceneData.ref.importSettings.calculateTangents = 1;
    sceneData.ref.importSettings.reverseHandedness = 0;
    sceneData.ref.importSettings.importEmbededTextures = 0;
    sceneData.ref.importSettings.importAnimations = 0;

    final Pointer<PrimitiveInitInfo> initInfo = malloc<PrimitiveInitInfo>();
    initInfo.ref.type = info.type.index;
    for (int i = 0; i < 3; i++) {
      initInfo.ref.segments[i] = info.segments[i];
      initInfo.ref.size[i] = info.size[i];
    }
    initInfo.ref.lod = info.lod;

    try {
      _createPrimitiveMesh(sceneData, initInfo);
      assert(sceneData.ref.data != nullptr && sceneData.ref.dataSize > 0);

      final Pointer<Uint8> dataPointer = sceneData.ref.data;
      final int dataSize = sceneData.ref.dataSize;
      final List<int> dataBuffer = dataPointer.asTypedList(dataSize);
      
      geometry.fromRawData(Uint8List.fromList(dataBuffer));

      free(sceneData.ref.data);

    } catch (err) {
      EditorLogger().log(LogLevel.error, "Failed to create ${capitalize(info.type.name.split('')[1])}", trace: StackTrace.current);
      debugLogger.e(err);
    } 
    finally {
      free(sceneData);
      free(initInfo);
    }
  }

  ContentToolsAPI._();
}
