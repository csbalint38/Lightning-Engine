import 'dart:convert';
import 'dart:typed_data';

Uint8List intToBytes(int value) {
    ByteData intData = ByteData(4);
    intData.setInt32(0, value, Endian.little);
    
    return intData.buffer.asUint8List();
  }
 Uint8List doubleToBytes(double value) {
    ByteData doubleData = ByteData(8);
    doubleData.setFloat32(0, value, Endian.little);

    return doubleData.buffer.asUint8List();
  }

Uint8List stringToBytes(String value) {
  return Uint8List.fromList(utf8.encode(value));
}