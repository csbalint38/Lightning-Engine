import 'package:editor/utilities/logger.dart';
import 'package:flutter/material.dart';

class Console extends StatefulWidget {
  const Console({super.key});

  @override
  State<Console> createState() => _ConsoleState();
}

class _ConsoleState extends State<Console> {
  final EditorLogger _logger = EditorLogger();

  @override
  void initState() {
    super.initState();

    _logger.log(LogLevel.info, "Info", trace: StackTrace.current);
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(8),
      child: ValueListenableBuilder(
          valueListenable: _logger.logsNotifier,
          builder: (context, value, _) {
            return ListView.builder(
              itemCount: _logger.filteredLogs.length,
              itemBuilder: (context, index) {
                return Text(value[index].toString());
              },
            );
          }),
    );
  }
}
