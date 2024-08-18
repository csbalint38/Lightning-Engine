abstract class Component {
  factory Component.fromXML() {
    throw UnimplementedError();
  }

  String toXML() {
    throw UnimplementedError();
  }
}

abstract class IMSComponent{}
abstract class MSComponent<T extends Component> implements IMSComponent {}