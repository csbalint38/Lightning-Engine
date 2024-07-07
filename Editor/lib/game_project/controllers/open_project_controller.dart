import 'package:editor/common/mvvm/viewmodel.dart';

class NewProjectController extends ViewModelBase {
  static final NewProjectController _newProjectController =
      NewProjectController._internal();

  factory NewProjectController() {
    return _newProjectController;
  }

  NewProjectController._internal();
}
