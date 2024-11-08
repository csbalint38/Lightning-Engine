import 'package:editor/components/script.dart';
import 'package:editor/components/transform.dart';

typedef CreationFunctionType = Function(Object);

enum ComponentType {
  trasnsform,
  script,
}

final class ComponentFactory {
  ComponentFactory._();

  static final List<CreationFunctionType> _functions = [
    (data) => Transform(),
    (data) => Script(name: data as String)
  ];

  static CreationFunctionType getCreationFunction(ComponentType componentType) {
    return _functions[componentType.index];
  }
}
