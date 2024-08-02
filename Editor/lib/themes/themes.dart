import 'package:docking/docking.dart';
import 'package:flutter/material.dart';

const TextStyle blackText = TextStyle(color: Colors.black);
const TextStyle whiteText = TextStyle(color: Colors.white);

ThemeData lightTheme = ThemeData(
  brightness: Brightness.light,
  primaryColor: Colors.blueGrey,
  textTheme: const TextTheme(
    headlineLarge: blackText,
    headlineMedium: blackText,
    headlineSmall: blackText,
    bodyLarge: blackText,
    bodyMedium: blackText,
    bodySmall: blackText,
  ),
  elevatedButtonTheme: ElevatedButtonThemeData(
    style: ButtonStyle(
      backgroundColor:
          WidgetStateProperty.resolveWith<Color>((Set<WidgetState> states) {
        if (states.contains(WidgetState.disabled)) {
          return Colors.blueGrey.withAlpha(180);
        }
        return Colors.blueGrey;
      }),
      overlayColor:
          const WidgetStatePropertyAll(Color.fromARGB(255, 109, 142, 158)),
      foregroundColor: const WidgetStatePropertyAll(Colors.white),
      shape: WidgetStatePropertyAll(
        RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(4),
        ),
      ),
    ),
  ),
  outlinedButtonTheme: OutlinedButtonThemeData(
    style: ButtonStyle(
      overlayColor: const WidgetStatePropertyAll(
        Color.fromARGB(27, 138, 169, 184),
      ),
      foregroundColor: const WidgetStatePropertyAll(Colors.blueGrey),
      shape: WidgetStatePropertyAll(
        RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(4),
        ),
      ),
      side: const WidgetStatePropertyAll(
        BorderSide(color: Colors.blueGrey),
      ),
    ),
  ),
  listTileTheme: const ListTileThemeData(
    selectedTileColor: Colors.blueGrey,
    selectedColor: Colors.white,
    textColor: Colors.blueGrey,
  ),
  inputDecorationTheme: const InputDecorationTheme(
    border: OutlineInputBorder(
      borderSide: BorderSide(
        color: Colors.blueGrey,
      ),
    ),
    focusedBorder: OutlineInputBorder(
      borderSide: BorderSide(
        color: Colors.blueGrey,
        width: 2,
      ),
    ),
    enabledBorder: OutlineInputBorder(
      borderSide: BorderSide(
        color: Colors.blueGrey,
      ),
    ),
    isDense: true,
  ),
  textSelectionTheme: TextSelectionThemeData(
    selectionColor: Colors.blue[200],
  ),
  iconTheme: const IconThemeData(
    color: Colors.blueGrey,
  ),
  iconButtonTheme: IconButtonThemeData(
    style: ButtonStyle(
      iconSize: const WidgetStatePropertyAll(16),
      padding: const WidgetStatePropertyAll(EdgeInsets.all(3)),
      iconColor: WidgetStateProperty.resolveWith<Color>(
        (Set<WidgetState> states) {
          if (states.contains(WidgetState.disabled)) {
            return Colors.black38;
          }
          return Colors.blueGrey;
        },
      ),
      backgroundColor: WidgetStateProperty.resolveWith<Color>(
        (Set<WidgetState> states) {
          if (states.contains(WidgetState.hovered) &&
              !states.contains(WidgetState.disabled)) {
            return const Color.fromARGB(50, 96, 125, 100);
          }
          return Colors.transparent;
        },
      ),
      minimumSize: const WidgetStatePropertyAll(Size(12, 12)),
    ),
  ),
  dividerColor: Colors.blueGrey.withAlpha(180),
);

ThemeData darkTheme = ThemeData(
  brightness: Brightness.dark,
  primaryColor: const Color.fromARGB(255, 5, 85, 151),
  textTheme: const TextTheme(
    headlineLarge: whiteText,
    headlineMedium: whiteText,
    headlineSmall: whiteText,
    bodyLarge: whiteText,
    bodyMedium: whiteText,
    bodySmall: whiteText,
  ),
);

extension CustomTheme on ThemeData {
  Color get borderColor =>
      brightness == Brightness.light ? Colors.black38 : Colors.black38;
  Color? get outlineColor =>
      brightness == Brightness.light ? Colors.blueGrey[100] : Colors.blueGrey;
  MultiSplitViewThemeData get msvTheme => brightness == Brightness.light
      ? MultiSplitViewThemeData(
          dividerPainter: DividerPainters.background(
            color: const Color.fromARGB(60, 122, 122, 122),
          ),
          dividerThickness: 6,
        )
      : MultiSplitViewThemeData();
  TabbedViewThemeData get tvTheme => brightness == Brightness.light
      ? TabbedViewThemeData(
          tabsArea: TabsAreaThemeData(
            color: Colors.blueGrey[100],
            middleGap: 6,
          ),
          menu: TabbedViewMenuThemeData(
            ellipsisOverflowText: true,
          ),
          tab: TabThemeData(
            normalButtonColor: Colors.blueGrey[800]!,
            padding: const EdgeInsets.fromLTRB(10, 0, 10, 0),
            buttonsOffset: 8,
            decoration: const BoxDecoration(
                shape: BoxShape.rectangle, color: Colors.blueGrey),
            textStyle: const TextStyle(
              color: Colors.white,
              fontSize: 12,
              height: 1.8,
            ),
            buttonIconSize: 15,
            buttonPadding: const EdgeInsets.fromLTRB(4, 0, 4, 0),
          ),
        )
      : TabbedViewThemeData();

  ButtonStyle get smallButton => brightness == Brightness.light
      ? const ButtonStyle(
          textStyle: WidgetStatePropertyAll(
            TextStyle(fontSize: 12),
          ),
          padding: WidgetStatePropertyAll(EdgeInsets.all(12)),
          tapTargetSize: MaterialTapTargetSize.shrinkWrap,
          minimumSize: WidgetStatePropertyAll(Size(10, 10)),
        )
      : const ButtonStyle();

  TextStyle get smallText => brightness == Brightness.light
      ? const TextStyle(
          fontSize: 12,
        )
      : const TextStyle();

  TextStyle get accentSmall => brightness == Brightness.light
      ? const TextStyle(
          fontSize: 12,
          color: Color.fromARGB(255, 109, 142, 158),
        )
      : const TextStyle();

  ButtonStyle get smallIcon => brightness == Brightness.light
      ? smallButton.copyWith(
          side: const WidgetStatePropertyAll(
            BorderSide(color: Colors.transparent, width: 0),
          ),
          iconSize: const WidgetStatePropertyAll(20),
          overlayColor: const WidgetStatePropertyAll(Colors.transparent),
          iconColor: WidgetStateProperty.resolveWith<Color>(
            (Set<WidgetState> states) {
              if (states.contains(WidgetState.disabled)) {
                return Colors.black38;
              }
              return Colors.blueGrey;
            },
          ),
        )
      : const ButtonStyle();
}
