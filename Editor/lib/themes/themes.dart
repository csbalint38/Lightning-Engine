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

ThemeData Lara = ThemeData(
  scaffoldBackgroundColor: Colors.lightGreen,
  primaryColor: Colors.pinkAccent,
  textTheme: const TextTheme(
    headlineLarge: whiteText,
    headlineMedium: whiteText,
    headlineSmall: whiteText,
    bodyLarge: whiteText,
    bodyMedium: whiteText,
    bodySmall: whiteText,
  ),
);
