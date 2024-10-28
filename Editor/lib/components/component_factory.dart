import 'package:editor/components/components.dart';
import 'package:editor/components/transform.dart';

enum ComponentType {
  transform,
  script,
}

extension ComponentFactory on Component {

  ComponentType toEnumType() {
    if(this is Transform) {
      return ComponentType.transform;
    }
    /*else if(this is Script) {
      return ComponentType.script;
    }*/
    else {
      throw ArgumentError('Unknown component type');
    }
  }
}