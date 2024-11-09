import 'package:editor/components/components.dart';
import 'package:editor/components/script.dart';
import 'package:editor/components/transform.dart';

typedef CreationFunctionType = Function(Object);

enum ComponentType {
  trasnsform,
  script,
}

extension ComponentFactory on Component {
  static final List<CreationFunctionType> _functions = [
    (data) => Transform(),
    (data) => Script(name: data as String)
  ];

  static CreationFunctionType getCreationFunction(ComponentType componentType) {
    return _functions[componentType.index];
  }

  ComponentType toEnumType() {
    if (this is Transform) {
      return ComponentType.trasnsform;
    } else if (this is Script) {
      return ComponentType.script;
    } else {
      throw ArgumentError('Unknown component type');
    }
  }
}
