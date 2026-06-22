import 'package:flutter/material.dart';

extension HexColorParsing on String {
  Color toAppColor({Color fallback = Colors.white}) {
    final normalized = replaceFirst('#', '').trim();
    if (normalized.length != 6) {
      return fallback;
    }

    final value = int.tryParse(normalized, radix: 16);
    if (value == null) {
      return fallback;
    }

    return Color(0xFF000000 | value);
  }
}
