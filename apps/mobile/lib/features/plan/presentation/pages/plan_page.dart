import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_decorations.dart';
import '../../../../core/theme/app_typography.dart';
import '../../../../core/theme/color_extensions.dart';
import '../../../../shared/widgets/phase_timeline.dart';
import '../../../../shared/widgets/section_label.dart';
import '../../../../shared/widgets/week_card.dart';
import '../../data/models/plan_models.dart';
import '../providers/plan_providers.dart';

class PlanPage extends ConsumerWidget {
  const PlanPage({super.key});

  static const _dayLabels = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activePlanAsync = ref.watch(activePlanProvider);

    return ColoredBox(
      color: AppColors.surface,
      child: activePlanAsync.when(
        data: (plan) {
          if (plan.weeks.isEmpty) {
            return const Center(
              child: Padding(
                padding: EdgeInsets.all(kPaddingH),
                child: Text(
                  'El plan activo no contiene semanas disponibles todavía.',
                  style: AppTypography.bodyMd,
                  textAlign: TextAlign.center,
                ),
              ),
            );
          }

          return CustomScrollView(
            slivers: [
              SliverToBoxAdapter(
                child: _PlanHeader(plan: plan),
              ),
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: kPaddingH),
                  child: _PhaseTimelineSection(plan: plan),
                ),
              ),
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(
                      kPaddingH, kGapLg, kPaddingH, 0),
                  child: _CurrentPhaseCard(plan: plan),
                ),
              ),
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(
                      kPaddingH, kGapLg, kPaddingH, kGapLg),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const SectionLabel('Semanas'),
                      const SizedBox(height: kGapMd),
                      ...plan.weeks.map(
                        (week) => Padding(
                          padding: const EdgeInsets.only(bottom: kGapMd),
                          child: WeekCard(week: week, labels: _dayLabels),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, _) => Center(
          child: Padding(
            padding: const EdgeInsets.all(kPaddingH),
            child: Text(
              error.toString().replaceFirst('Exception: ', ''),
              style: AppTypography.bodyMd.copyWith(color: AppColors.error),
              textAlign: TextAlign.center,
            ),
          ),
        ),
      ),
    );
  }
}

class _PlanHeader extends StatelessWidget {
  const _PlanHeader({
    required this.plan,
  });

  final UserPlanDetailDto plan;

  @override
  Widget build(BuildContext context) {
    final dateFormatter = DateFormat('d MMM yyyy', 'es');

    return Container(
      decoration: AppDecorations.headerGradient,
      padding:
          const EdgeInsets.fromLTRB(kPaddingH, kPaddingTop, kPaddingH, kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SectionLabel('Plan activo'),
          const SizedBox(height: kGapSm),
          Text(plan.name, style: AppTypography.headingLg),
          const SizedBox(height: kGapMd),
          Wrap(
            spacing: kGapSm,
            runSpacing: kGapSm,
            children: [
              _InfoBadge(label: plan.difficultyLevelCode.toUpperCase()),
              _InfoBadge(label: '${plan.weeks.length} semanas'),
              _InfoBadge(
                label:
                    '${dateFormatter.format(plan.startDate)} · ${plan.endDate != null ? dateFormatter.format(plan.endDate!) : 'Activo'}',
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _PhaseTimelineSection extends StatelessWidget {
  const _PhaseTimelineSection({
    required this.plan,
  });

  final UserPlanDetailDto plan;

  @override
  Widget build(BuildContext context) {
    final grouped = <String, List<PlanWeekDto>>{};
    for (final week in plan.weeks) {
      grouped
          .putIfAbsent('${week.phaseName}|${week.phaseColorHex}', () => [])
          .add(week);
    }

    final segments = grouped.entries.map((entry) {
      final weeks = entry.value;
      return PhaseTimelineSegment(
        label: weeks.first.phaseName,
        color:
            weeks.first.phaseColorHex.toAppColor(fallback: AppColors.secondary),
        startWeek: weeks.first.weekNumber,
        endWeek: weeks.last.weekNumber,
      );
    }).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const SectionLabel('Timeline'),
        const SizedBox(height: kGapMd),
        PhaseTimeline(
          segments: segments,
          currentWeek: _resolveCurrentWeek(plan),
          totalWeeks: plan.weeks.length,
        ),
      ],
    );
  }

  int _resolveCurrentWeek(UserPlanDetailDto plan) {
    if (plan.weeks.isEmpty) {
      return 1;
    }

    final elapsedDays = DateTime.now().difference(plan.startDate).inDays;
    return ((elapsedDays < 0 ? 0 : elapsedDays) ~/ 7) + 1;
  }
}

class _CurrentPhaseCard extends StatelessWidget {
  const _CurrentPhaseCard({
    required this.plan,
  });

  final UserPlanDetailDto plan;

  @override
  Widget build(BuildContext context) {
    final currentWeekNumber =
        _resolveCurrentWeek(plan).clamp(1, plan.weeks.length);
    final currentWeek = plan.weeks.firstWhere(
      (week) => week.weekNumber == currentWeekNumber,
      orElse: () => plan.weeks.first,
    );
    final phaseColor =
        currentWeek.phaseColorHex.toAppColor(fallback: AppColors.secondary);

    return Container(
      padding: const EdgeInsets.all(kGapLg),
      decoration: BoxDecoration(
        color: phaseColor.withValues(alpha: 0.07),
        borderRadius: BorderRadius.circular(kRadiusLg),
        border: Border.all(color: phaseColor.withValues(alpha: 0.2)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SectionLabel('Fase actual'),
          const SizedBox(height: kGapSm),
          Text(
            currentWeek.phaseName,
            style: AppTypography.headingSm.copyWith(color: phaseColor),
          ),
          const SizedBox(height: 6),
          Text(
            'Semana ${currentWeek.weekNumber} de ${plan.weeks.length}',
            style:
                AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
          ),
        ],
      ),
    );
  }

  int _resolveCurrentWeek(UserPlanDetailDto plan) {
    final elapsedDays = DateTime.now().difference(plan.startDate).inDays;
    return ((elapsedDays < 0 ? 0 : elapsedDays) ~/ 7) + 1;
  }
}

class _InfoBadge extends StatelessWidget {
  const _InfoBadge({
    required this.label,
  });

  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: AppColors.surfaceCard,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: Colors.white.withValues(alpha: 0.06)),
      ),
      child: Text(label,
          style: AppTypography.bodySm.copyWith(color: AppColors.textPrimary)),
    );
  }
}
