import 'package:editor/common/scalar_box.dart';
import 'package:editor/common/vector_notifier.dart';
import 'package:flutter/material.dart';

enum VectorNotation { xyz, rgb, abc }

class VectorBox extends StatelessWidget {
  final FocusNode globalFocus;
  final Vector3Notifier notifier;
  final bool isEnabled;

  const VectorBox(this.globalFocus, this.notifier,
      {super.key, this.isEnabled = true});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        ValueListenableBuilder(
          valueListenable: notifier,
          builder: (context, _, __) {
            return Row(
              children: [
                Text(
                  'X',
                  style: !isEnabled
                      ? TextStyle(color: Theme.of(context).disabledColor)
                      : null,
                ),
                const SizedBox(width: 5),
                ScalarBox(
                  globalFocus,
                  (x) => notifier.x = x,
                  notifier.x,
                  isEnabled: isEnabled,
                ),
                const SizedBox(width: 15),
                Text(
                  'Y',
                  style: !isEnabled
                      ? TextStyle(color: Theme.of(context).disabledColor)
                      : null,
                ),
                const SizedBox(width: 5),
                ScalarBox(
                  globalFocus,
                  (y) => notifier.y = y,
                  notifier.y,
                  isEnabled: isEnabled,
                ),
                const SizedBox(width: 15),
                Text(
                  'Z',
                  style: !isEnabled
                      ? TextStyle(color: Theme.of(context).disabledColor)
                      : null,
                ),
                const SizedBox(width: 5),
                ScalarBox(
                  globalFocus,
                  (z) => notifier.z = z,
                  notifier.z,
                  isEnabled: isEnabled,
                ),
              ],
            );
          },
        ),
      ],
    );
  }
}
