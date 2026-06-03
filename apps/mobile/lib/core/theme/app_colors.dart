import 'package:flutter/material.dart';

class AppColors {
  static const surface = Color(0xFF0F0F0F);
  static const surfaceVariant = Color(0xFF1A1A1A);
  static const surfaceCard = Color(0xFF242424);

  static const textPrimary = Color(0xFFF5F5F5);
  static const textSecondary = Color(0xFFA0A0A0);

  static const primary = Color(0xFFE8FF47);
  static const secondary = Color(0xFFFF6B35);
  static const tertiary = Color(0xFF4FC3F7);
  static const error = Color(0xFFFF4444);
  static const success = Color(0xFF66BB6A);

  static const sessionArc = Color(0xFF4FC3F7);
  static const sessionHangboard = Color(0xFFFF6B35);
  static const sessionCampus = Color(0xFFFF4444);
  static const sessionBoulder = Color(0xFFE8FF47);
  static const sessionOutdoor = Color(0xFF66BB6A);
  static const sessionRest = Color(0xFF5C5C5C);

  static const phaseBase = Color(0xFF4FC3F7);
  static const phaseFuerza = Color(0xFFFF6B35);
  static const phasePotencia = Color(0xFFFF4444);
  static const phaseResistencia = Color(0xFF66BB6A);
  static const phasePerformance = Color(0xFFE8FF47);

  static Color rpeColor(int rpe) {
    if (rpe <= 3) return tertiary;
    if (rpe <= 5) return success;
    if (rpe <= 7) return primary;
    if (rpe <= 8) return secondary;
    return error;
  }
}
