import 'dart:convert';
import 'dart:io';
import 'package:path/path.dart' as p;

enum ConfigProps{ENGINE_PATH}

class Config {
  final File dataFile = File(p.join(Platform.environment["LOCALAPPDATA"]!, 'LightningEditor', 'environment.json'));
  late Map<String, dynamic> data;

  Config() {
    if(dataFile.existsSync()) {
      data = jsonDecode(dataFile.readAsStringSync());
    }
    else data = Map<String, dynamic>();
  }

  dynamic read(ConfigProps prop) {
    switch(prop) {
      case ConfigProps.ENGINE_PATH:
        return data.containsKey('engine_path') ? data['engine_path'] : null;
    }
  }

  void write(ConfigProps prop, dynamic value) {
    switch(prop) {
      case ConfigProps.ENGINE_PATH:
        data['engine_path'] = value;
        _writeToFile();
    }
  }

  void _writeToFile() {
    final String json = jsonEncode(data);
    dataFile.writeAsStringSync(json, mode: FileMode.write);
  }
}