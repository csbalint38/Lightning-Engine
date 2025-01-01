import 'dart:convert';
import 'dart:math';

String generateRandomString({ int length = 8 }) {
  length = length <= 0 ? 8 : length;
  var random = Random.secure();
  var values = List<int>.generate(length, (i) => random.nextInt(255));
  return base64Url.encode(values);
}