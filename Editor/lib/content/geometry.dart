import 'package:editor/content/asset.dart';

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
  const Geometry() : super(AssetType.mesh);

  void fromRawData(List<int> data) {
    assert(data.isNotEmpty);
  }
}