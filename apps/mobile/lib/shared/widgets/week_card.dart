import 'package:flutter/material.dart';

import '../../core/constants/layout_constants.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_decorations.dart';
import '../../core/theme/app_typography.dart';
import '../../core/theme/color_extensions.dart';
import '../../features/plan/data/models/plan_models.dart';
import 'day_chip.dart';

class WeekCard extends StatefulWidget {
  const WeekCard({
    super.key,
    required this.week,
    required this.labels,
  });

  final PlanWeekDto week;
  final List<String> labels;

  @override
  State<WeekCard> createState() => _WeekCardState();
}

class _WeekCardState extends State<WeekCard> {
  bool _expanded = false;

  @override
  Widget build(BuildContext context) {
    final phaseColor =
        widget.week.phaseColorHex.toAppColor(fallback: AppColors.secondary);

    return AnimatedSize(
      duration: const Duration(milliseconds: 220),
      curve: Curves.easeOutCubic,
      child: DecoratedBox(
        decoration: AppDecorations.darkCard,
        child: InkWell(
          borderRadius: BorderRadius.circular(kRadiusLg),
          onTap: () => setState(() => _expanded = !_expanded),
          child: Padding(
            padding: const EdgeInsets.all(kGapMd),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Text('#${widget.week.weekNumber}',
                        style: AppTypography.headingSm),
                    const SizedBox(width: kGapSm),
                    Expanded(
                      child: Text(
                        widget.week.phaseName,
                        style: AppTypography.headingSm
                            .copyWith(color: phaseColor, fontSize: 18),
                      ),
                    ),
                    if (widget.week.isDeload)
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 10, vertical: 5),
                        decoration: BoxDecoration(
                          color: phaseColor.withValues(alpha: 0.14),
                          borderRadius: BorderRadius.circular(999),
                        ),
                        child: Text(
                          'DELOAD',
                          style: AppTypography.labelCaps
                              .copyWith(color: phaseColor),
                        ),
                      ),
                    const SizedBox(width: kGapSm),
                    Icon(
                      _expanded
                          ? Icons.keyboard_arrow_up_rounded
                          : Icons.keyboard_arrow_down_rounded,
                      color: AppColors.textSecondary,
                    ),
                  ],
                ),
                const SizedBox(height: kGapMd),
                ClipRRect(
                  borderRadius: BorderRadius.circular(999),
                  child: LinearProgressIndicator(
                    value: (widget.week.progressPct / 100).clamp(0, 1),
                    minHeight: 8,
                    backgroundColor: AppColors.surfaceCard,
                    valueColor: AlwaysStoppedAnimation<Color>(phaseColor),
                  ),
                ),
                const SizedBox(height: 6),
                Text(
                  '${widget.week.progressPct.toStringAsFixed(0)}% completado',
                  style: AppTypography.bodySm,
                ),
                if (_expanded) ...[
                  const SizedBox(height: kGapMd),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: List.generate(widget.week.days.length, (index) {
                      final day = widget.week.days[index];
                      final color = (day.sessionColorHex ?? '')
                          .toAppColor(fallback: AppColors.surfaceCard);
                      final label = index < widget.labels.length
                          ? widget.labels[index]
                          : day.label;
                      return DayChip(
                        label: label,
                        state: day.state,
                        sessionColor: color,
                      );
                    }),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
