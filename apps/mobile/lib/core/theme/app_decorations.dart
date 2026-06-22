import 'package:flutter/material.dart';

import '../constants/layout_constants.dart';
import 'app_colors.dart';

class AppDecorations {
  static BoxDecoration sessionCard(Color sessionColor) => BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(kRadiusLg),
        border:
            Border.all(color: sessionColor.withValues(alpha: 0.2), width: 1.5),
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppColors.surfaceVariant,
            sessionColor.withValues(alpha: 0.07),
          ],
          stops: const [0.6, 1.0],
        ),
      );

  static BoxDecoration get darkCard => BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(kRadiusLg),
        border: Border.all(color: Colors.white.withValues(alpha: 0.06)),
      );

  static BoxDecoration get headerGradient => const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [AppColors.surfaceVariant, AppColors.surface],
        ),
      );
}
