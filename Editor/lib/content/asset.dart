enum AssetType {
  unknown,
  animatin,
  audio,
  material,
  mesh,
  skeleton,
  texture,
}

abstract class Asset {
  final AssetType type;

  const Asset(this.type);
}