import 'dart:typed_data';

import 'package:editor/content/asset.dart';
import 'package:editor/utilities/id.dart';
import 'package:editor/utilities/random_string_generator.dart';

enum PrimitiveMeshType { plane, cube, uvSphere, icoSphere, cylinder, capsule }

class PrimitiveInitInfo {
  final PrimitiveMeshType type;
  final List<int> segments;
  final List<double> size;
  final int lod;

  PrimitiveInitInfo(this.type, this.segments, this.size, this.lod);
}

class Mesh {
  final int vertexSize;
  final int vertexCount;
  final int indexSize;
  final int indexCount;
  final List<int> verticies;
  final List<int> indicies;

  Mesh(this.vertexSize, this.vertexCount, this.indexSize, this.indexCount, this.verticies, this.indicies);
}

class MeshLOD {
  final String name;
  final double lodTreshold;
  final List<Mesh> meshes = <Mesh>[];

  MeshLOD(this.name, this.lodTreshold);
}

class LODGroup {
  final String name;
  final List<MeshLOD> lods;

  LODGroup(this.name, this.lods);
}

class Geometry extends Asset {
  final List<LODGroup> _lodGroups = const <LODGroup>[];

  const Geometry() : super(AssetType.mesh);

  void fromRawData(Uint8List data) {
    assert(data.isNotEmpty);

    _lodGroups.clear();

    ByteData buffer = ByteData.sublistView(data);

    int offset = 4; // Skip scene name string
    int numLodGroups = buffer.getInt32(offset, Endian.little);             // Get number of LODs
    assert(numLodGroups > 0);
    offset += 4;

    for (int i = 0; i < numLodGroups; i++) {
      int nameLength = buffer.getInt32(offset, Endian.little); // Read LOD group name length
      offset += 4;

      String lodGroupName;
      if(nameLength > 0) {
        lodGroupName = String.fromCharCodes(data.sublist(offset, offset + nameLength));
        offset += nameLength;
      }
      else {
        lodGroupName = generateRandomString();
      }

      int numMeshes = buffer.getInt32(offset, Endian.little); // Get number of meshes in this LOD group
      assert(numMeshes > 0);
      offset += 4;

      List<MeshLOD> lods = _readMeshLODs(numMeshes, ByteData.sublistView(data, offset));
      LODGroup lodGroup = LODGroup(lodGroupName, lods);
      _lodGroups.add(lodGroup);
    }
  }

  LODGroup getLODGroup({ int lodGroup = 0 }) {
    assert(lodGroup >= 0 && lodGroup < _lodGroups.length);

    return _lodGroups[lodGroup];
  }

  static List<MeshLOD> _readMeshLODs(int numMeshes, ByteData buffer) {
    List<int> lodIds = const <int>[];
    List<MeshLOD> lods = const <MeshLOD>[];

    for(int i = 0; i < numMeshes; i++) {
      _readMeshes(buffer, lodIds, lods);
    }

    return lods;
  }

  static void _readMeshes(ByteData buffer, List<int> lodIds, List<MeshLOD> lods) {
    int offset = 0;

    int meshNameLength = buffer.getInt32(offset, Endian.little);
    offset += 4;

    String meshName;
    if(meshNameLength > 0) {
      meshName = String.fromCharCodes(buffer.buffer.asUint8List(offset, meshNameLength));
      offset += meshNameLength;
    }
    else {
      meshName = "mesh_$generateRandomString()";
    }

    int lodId = buffer.getInt32(offset, Endian.little);
    offset += 4;
    int vertexSize = buffer.getInt32(offset, Endian.little);
    offset += 4;
    int vertexCount = buffer.getInt32(offset, Endian.little);
    offset += 4;
    int indexSize = buffer.getInt32(offset, Endian.little);
    offset += 4;
    int indexCount = buffer.getInt32(offset, Endian.little);
    offset += 4;
    double lodTreshold = buffer.getFloat32(offset, Endian.little);
    offset += 4;

    int vertexBufferSize = vertexSize * vertexCount;
    int indexBufferSize = indexSize * indexCount;

    List<int> verticies = List<int>.generate(vertexBufferSize, (i) => buffer.getInt32(offset + i * 4, Endian.little));
    offset += vertexBufferSize * 4;
    List<int> indicies = List<int>.generate(indexBufferSize, (i) => buffer.getInt32(offset + i * 4, Endian.little));
    offset += indexBufferSize * 4;

    Mesh mesh = Mesh(vertexSize, vertexCount, indexSize, indexCount, verticies, indicies);

    MeshLOD lod;

    if(Id.isValid(lodId) && lodIds.contains(lodId)) {
      lod = lods[lodIds.indexOf(lodId)];
    }
    else {
      lodIds.add(lodId);
      lod = MeshLOD(meshName, lodTreshold);
      lods.add(lod);
    }

    lod.meshes.add(mesh);
  }
}