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
    ));

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
}
