import 'dart:convert';
import 'dart:typed_data';

Uint8List intToBytes(int value) {
  ByteData intData = ByteData(4);
  intData.setUint32(0, value, Endian.little);

  return intData.buffer.asUint8List();
}

Uint8List doubleToBytes(double value) {
  ByteData doubleData = ByteData(4);
  doubleData.setFloat32(0, value, Endian.little);

  return doubleData.buffer.asUint8List();
}

Uint8List stringToBytes(String value) {
  final Uint8List data = utf8.encode(value);
  final Uint8List lengthPrefix = intToBytes(data.length);
  return Uint8List.fromList(lengthPrefix + data);
}
