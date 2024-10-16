import 'package:editor/themes/themes.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class ScalarBox extends StatelessWidget {
  final FocusNode globalFocus;
  final Function(double) callback;
  final FocusNode _localFocus = FocusNode();
  final TextEditingController _controller = TextEditingController();

  ScalarBox(this.globalFocus, this.callback, double? initValue, {super.key}) {
    _controller.text = initValue != null ? initValue.toString() : '---';
  }

  void _update(String value) {
    if (double.tryParse(value) != null) {
      callback(double.parse(value));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: const BoxConstraints(maxWidth: 65),
      child: Focus(
        focusNode: _localFocus,
        onKeyEvent: (node, event) {
          if (event is KeyDownEvent &&
              event.logicalKey == LogicalKeyboardKey.escape) {
            _localFocus.unfocus();
            globalFocus.requestFocus();
            return KeyEventResult.handled;
          }
          return KeyEventResult.ignored;
        },
        child: TextField(
          textAlignVertical: TextAlignVertical.center,
          cursorHeight: 14,
          maxLines: 1,
          controller: _controller,
          style: Theme.of(context).smallText,
          decoration: InputDecoration(
            contentPadding:
                const EdgeInsets.symmetric(vertical: 7, horizontal: 4),
            isDense: true,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(2),
              borderSide: const BorderSide(width: 1),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(2),
              borderSide: const BorderSide(width: 1, color: Colors.blueGrey),
            ),
          ),
          inputFormatters: [
            FilteringTextInputFormatter.allow(
              RegExp(r"[0-9.]"),
            ),
            TextInputFormatter.withFunction(
              (oldValue, newValue) {
                final text = newValue.text;
                return text.isEmpty
                    ? newValue
                    : double.tryParse(text) == null
                        ? oldValue
                        : newValue;
              },
            ),
          ],
          onSubmitted: (value) {
            globalFocus.requestFocus();
            _update(value);
          },
          onTapOutside: (event) {
            globalFocus.requestFocus();
            _update(_controller.text);
          },
        ),
      ),
    );
  }
}
