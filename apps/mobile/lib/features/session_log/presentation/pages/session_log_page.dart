import 'package:flutter/material.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_typography.dart';

class SessionLogPage extends StatelessWidget {
  const SessionLogPage({
    super.key,
    required this.weekId,
    required this.dayOfWeek,
  });

  final int weekId;
  final int dayOfWeek;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.surface,
      appBar: AppBar(title: const Text('Registro de sesión')),
      body: Padding(
        padding: const EdgeInsets.all(kPaddingH),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Flujo de sesión', style: AppTypography.headingLg),
            const SizedBox(height: kGapSm),
            Text('Semana $weekId · Día $dayOfWeek',
                style: AppTypography.bodySm),
            const SizedBox(height: kGapMd),
            const Text(
              'La navegación del dashboard ya llega correctamente a esta ruta. El flujo detallado se implementa en su agente correspondiente.',
              style: AppTypography.bodyMd,
            ),
          ],
        ),
      ),
    );
  }
}
