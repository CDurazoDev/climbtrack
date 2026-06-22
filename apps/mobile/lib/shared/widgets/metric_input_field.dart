import 'package:flutter/material.dart';

import '../../core/constants/layout_constants.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_typography.dart';

class MetricInputField extends StatelessWidget {
  const MetricInputField({
    super.key,
    required this.label,
    required this.value,
    required this.onChanged,
    this.unit,
    this.step = 1,
    this.min,
    this.max,
    this.allowNegative = false,
  });

  final String label;
  final num value;
  final ValueChanged<num> onChanged;
  final String? unit;
  final num step;
  final num? min;
  final num? max;
  final bool allowNegative;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: BoxDecoration(
        color: AppColors.surfaceCard,
        borderRadius: BorderRadius.circular(kRadiusMd),
        border: Border.all(color: Colors.white.withValues(alpha: 0.06)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: AppTypography.bodySm.copyWith(color: AppColors.textPrimary),
          ),
          const SizedBox(height: kGapMd),
          Row(
            children: [
              _StepperButton(
                icon: Icons.remove_rounded,
                onTap: () => onChanged(_clamp(value - step)),
              ),
              Expanded(
                child: Center(
                  child: RichText(
                    text: TextSpan(
                      style: AppTypography.metricLg,
                      children: [
                        TextSpan(text: _formatValue(value)),
                        if (unit != null && unit!.isNotEmpty)
                          TextSpan(
                            text: ' $unit',
                            style: AppTypography.metricSm.copyWith(
                              color: AppColors.textSecondary,
                              fontSize: 14,
                            ),
                          ),
                      ],
                    ),
                  ),
                ),
              ),
              _StepperButton(
                icon: Icons.add_rounded,
                onTap: () => onChanged(_clamp(value + step)),
              ),
            ],
          ),
        ],
      ),
    );
  }

  num _clamp(num candidate) {
    var result = candidate;
    if (!allowNegative && result < 0) {
      result = 0;
    }
    if (min != null && result < min!) {
      result = min!;
    }
    if (max != null && result > max!) {
      result = max!;
    }
    return result;
  }

  String _formatValue(num current) {
    if (current == current.roundToDouble()) {
      return current.toStringAsFixed(0);
    }

    return current.toStringAsFixed(1);
  }
}

class _StepperButton extends StatelessWidget {
  const _StepperButton({
    required this.icon,
    required this.onTap,
  });

  final IconData icon;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(999),
      child: Container(
        width: 40,
        height: 40,
        decoration: BoxDecoration(
          color: AppColors.surfaceVariant,
          shape: BoxShape.circle,
          border: Border.all(color: Colors.white.withValues(alpha: 0.08)),
        ),
        child: Icon(icon, color: AppColors.textPrimary),
      ),
    );
  }
}
