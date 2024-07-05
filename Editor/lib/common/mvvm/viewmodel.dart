import 'observer.dart';
import 'package:editor/utilities/logger.dart';

abstract class ViewModelBase {
  final List<EventObserver> _observerList = List.empty(growable: true);

  void subscribe(EventObserver o) {
    if (_observerList.contains(o)) return;

    _observerList.add(o);
  }

  bool unsubscribe(EventObserver o) {
    if (_observerList.contains(o)) {
      _observerList.remove(o);
      return true;
    }
    return false;
  }

  void notify(ViewEvent event) {
    debugLogger.i("Event occured --- $event");
    for (var element in _observerList) {
      element.notify(event);
    }
  }
}
