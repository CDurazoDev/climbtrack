import 'package:flutter/material.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../data/models/day_entry_model.dart';

class DashboardDayChip extends StatelessWidget {
  const DashboardDayChip({
    super.key,
    required this.entry,
  });

  final DayEntryModel entry;

  @override
  Widget build(BuildContext context) {
    final sessionColor = _parseColor(entry.sessionColorHex) ?? AppColors.surfaceCard;
    final scheme = _resolveScheme(entry.state, sessionColor);

    return Container(
      width: 42,
      padding: const EdgeInsets.symmetric(vertical: 10),
      decoration: BoxDecoration(
        color: scheme.background,
        borderRadius: BorderRadius.circular(kRadiusMd),
        border: Border.all(color: scheme.border),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            entry.label,
            style: Theme.of(context).textTheme.labelSmall?.copyWith(
                  color: scheme.foreground,
                  fontWeight: FontWeight.w700,
                ),
          ),
          const SizedBox(height: 4),
          Icon(
            scheme.icon,
            size: 14,
            color: scheme.foreground,
          ),
        ],
      ),
    );
  }

  Color? _parseColor(String? hexColor) {
    if (hexColor == null || hexColor.isEmpty) {
      return null;
    }

    final normalized = hexColor.replaceFirst('#', '');
    if (normalized.length != 6) {
      return null;
    }

    return Color(int.parse('FF$normalized', radix: 16));
  }

  _DayChipScheme _resolveScheme(String state, Color sessionColor) {
    switch (state) {
      case 'completed':
        return _DayChipScheme(
          background: sessionColor.withValues(alpha: 0.22),
          border: sessionColor,
          foreground: sessionColor,
          icon: Icons.check_rounded,
        );
      case 'today':
        return _DayChipScheme(
          background: AppColors.primary.withValues(alpha: 0.14),
          border: AppColors.primary,
          foreground: AppColors.primary,
          icon: Icons.radio_button_checked_rounded,
        );
      case 'failed':
        return _DayChipScheme(
          background: AppColors.error.withValues(alpha: 0.12),
          border: AppColors.error.withValues(alpha: 0.45),
          foreground: AppColors.error,
          icon: Icons.close_rounded,
        );
      case 'pending':
        return _DayChipScheme(
          background: sessionColor.withValues(alpha: 0.08),
          border: sessionColor.withValues(alpha: 0.3),
          foreground: AppColors.textPrimary,
          icon: Icons.schedule_rounded,
        );
      default:
        return const _DayChipScheme(
          background: AppColors.surfaceCard,
          border: Colors.white12,
          foreground: AppColors.textSecondary,
          icon: Icons.bedtime_rounded,
        );
    }
  }
}

class _DayChipScheme {
  const _DayChipScheme({
    required this.background,
    required this.border,
    required this.foreground,
    required this.icon,
  });

  final Color background;
  final Color border;
  final Color foreground;
  final IconData icon;
}
