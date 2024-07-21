import 'dart:collection';

typedef ListChangeListener<T> = void Function(List<T> list);

class ObservableList<T> extends ListBase<T> {
  final List<T> _innerList = [];
  final List<ListChangeListener<T>> _listeners = [];

  void addListener(ListChangeListener<T> listener) {
    _listeners.add(listener);
  }

  void removeListener(ListChangeListener<T> listener) {
    _listeners.remove(listener);
  }

  void _notify() {
    for (final listener in _listeners) {
      listener(UnmodifiableListView<T>(_innerList));
    }
  }

  @override
  int get length => _innerList.length;

  @override
  set length(int newLength) {
    _innerList.length = newLength;
    _notify();
  }

  @override
  T operator [](int index) => _innerList[index];

  @override
  void operator []=(int index, T value) {
    _innerList[index] = value;
    _notify();
  }

  @override
  void add(T element) {
    _innerList.add(element);
    _notify();
  }

  @override
  void insert(int index, T element) {
    _innerList.insert(index, element);
    _notify();
  }

  @override
  bool remove(Object? element) {
    final result = _innerList.remove(element);
    if (result) _notify();
    return result;
  }

  @override
  T removeAt(int index) {
    final result = _innerList.removeAt(index);
    _notify();
    return result;
  }

  @override
  void clear() {
    _innerList.clear();
    _notify();
  }
}
