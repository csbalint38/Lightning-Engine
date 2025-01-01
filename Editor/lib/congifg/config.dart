import 'dart:convert';
import 'dart:io';
import 'package:path/path.dart' as p;

enum ConfigProps { enginePath, contentToolsPath }

class Config {
  final File dataFile = File(p.join(Platform.environment["LOCALAPPDATA"]!,
      'LightningEditor', 'environment.json'));
  late Map<String, dynamic> data;

  Config() {
    if (dataFile.existsSync()) {
      data = jsonDecode(dataFile.readAsStringSync());
    } else {
      data = <String, dynamic>{};
    }
  }

  T? read<T>(ConfigProps prop) {
    switch (prop) {
      case ConfigProps.enginePath:
        return data.containsKey('engine_path')
            ? data['engine_path'] as T
            : null;
      case ConfigProps.contentToolsPath:
        return data.containsKey('content_tools_path')
            ? data['content_tools_path'] as T
            : null;
    }
  }

  void write(ConfigProps prop, dynamic value) {
    switch (prop) {
      case ConfigProps.enginePath:
        data['engine_path'] = value;
        _writeToFile();
      case ConfigProps.contentToolsPath:
        data['content_tools_path'] = value;
        _writeToFile();
    }
  }

  void _writeToFile() {
    final String json = jsonEncode(data);
    dataFile.writeAsStringSync(json, mode: FileMode.write);
  }
}
