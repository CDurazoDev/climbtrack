import 'package:flutter/material.dart';

import '../../core/constants/layout_constants.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_typography.dart';

class DayChip extends StatelessWidget {
  const DayChip({
    super.key,
    required this.label,
    required this.state,
    this.sessionColor,
    this.onTap,
  });

  final String label;
  final String state;
  final Color? sessionColor;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final isToday = state == 'today';
    final backgroundColor = switch (state) {
      'completed' => sessionColor ?? AppColors.primary,
      'failed' => AppColors.error.withValues(alpha: 0.2),
      'rest' => AppColors.surfaceCard,
      _ => Colors.transparent,
    };

    final borderColor = switch (state) {
      'today' => AppColors.primary,
      'pending' => Colors.white.withValues(alpha: 0.2),
      'rest' => Colors.white.withValues(alpha: 0.06),
      _ => Colors.transparent,
    };

    final foregroundColor = switch (state) {
      'completed' => Colors.white,
      'failed' => AppColors.error,
      'rest' => AppColors.textSecondary,
      _ => AppColors.textPrimary,
    };

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(kRadiusMd),
        child: Container(
          width: 42,
          padding: const EdgeInsets.symmetric(vertical: 6),
          decoration: BoxDecoration(
            color: backgroundColor,
            borderRadius: BorderRadius.circular(kRadiusMd),
            border: Border.all(color: borderColor, width: isToday ? 2 : 1),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(label,
                  style: AppTypography.bodySm
                      .copyWith(color: AppColors.textSecondary)),
              const SizedBox(height: 6),
              SizedBox(
                height: 18,
                child: Center(
                  child: _buildStateContent(foregroundColor),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildStateContent(Color color) {
    switch (state) {
      case 'completed':
        return Icon(Icons.check_rounded, size: 16, color: color);
      case 'failed':
        return Icon(Icons.close_rounded, size: 16, color: color);
      case 'rest':
        return Text('—',
            style:
                AppTypography.headingSm.copyWith(color: color, fontSize: 16));
      default:
        return Container(
          width: 10,
          height: 10,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            color: state == 'today'
                ? AppColors.primary.withValues(alpha: 0.14)
                : Colors.transparent,
          ),
        );
    }
  }
}
