abstract class Component {
  factory Component.fromXML() {
    throw UnimplementedError();
  }

  String toXML() {
    throw UnimplementedError();
  }
}
