import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_decorations.dart';
import '../../../../core/theme/app_typography.dart';
import '../../../../core/theme/color_extensions.dart';
import '../../../auth/data/models/auth_tokens_model.dart';
import '../../../auth/presentation/providers/auth_state.dart';
import '../../../plan/data/models/plan_models.dart';
import '../../../../shared/widgets/day_chip.dart';
import '../../../../shared/widgets/section_label.dart';
import '../providers/dashboard_providers.dart';
import '../../data/models/dashboard_models.dart';

class DashboardPage extends ConsumerWidget {
  const DashboardPage({super.key});

  static const _dayLabels = ['L', 'M', 'X', 'J', 'V', 'S', 'D'];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final todayAsync = ref.watch(todaySessionProvider);
    final weekAsync = ref.watch(currentWeekProvider);
    final summaryAsync = ref.watch(statsSummaryProvider);
    final user = ref.watch(authStateProvider).valueOrNull?.maybeWhen(
          authenticated: (user) => user,
          orElse: () => null,
        );

    return ColoredBox(
      color: AppColors.surface,
      child: CustomScrollView(
        slivers: [
          SliverToBoxAdapter(
            child: _DashboardHeader(
              user: user,
              currentWeek: weekAsync.valueOrNull,
            ),
          ),
          SliverToBoxAdapter(
            child: Padding(
              padding:
                  const EdgeInsets.fromLTRB(kPaddingH, kGapMd, kPaddingH, 0),
              child: todayAsync.when(
                data: (session) {
                  if (session != null) {
                    return _TodaySessionCard(
                      session: session,
                      planWeekId: weekAsync.valueOrNull?.id,
                      dayOfWeek: _resolveTodayDayOfWeek(weekAsync.valueOrNull),
                    );
                  }

                  final plannedSession =
                      _resolvePlannedTodaySession(weekAsync.valueOrNull);
                  if (plannedSession != null) {
                    return _PlannedTodaySessionCard(
                      session: plannedSession,
                      planWeekId: weekAsync.valueOrNull?.id,
                      dayOfWeek: _resolveTodayDayOfWeek(weekAsync.valueOrNull),
                    );
                  }

                  return const _RestDayCard();
                },
                loading: () => const _SessionCardSkeleton(),
                error: (error, _) => _ErrorCard(message: error.toString()),
              ),
            ),
          ),
          SliverToBoxAdapter(
            child: _WeekStrip(
              weekAsync: weekAsync,
              labels: _dayLabels,
            ),
          ),
          SliverToBoxAdapter(
            child: _QuickStats(summaryAsync: summaryAsync),
          ),
          const SliverPadding(padding: EdgeInsets.only(bottom: kGapLg)),
        ],
      ),
    );
  }

  int _resolveTodayDayOfWeek(PlanWeekDto? week) {
    final todayIndex = ((DateTime.now().weekday + 6) % 7);
    if (week == null) {
      return todayIndex;
    }

    final currentIndex = week.days.indexWhere((day) => day.state == 'today');
    return currentIndex >= 0 ? currentIndex : todayIndex;
  }

  _PlannedTodaySession? _resolvePlannedTodaySession(PlanWeekDto? week) {
    if (week == null) {
      return null;
    }

    final dayIndex = _resolveTodayDayOfWeek(week);
    if (dayIndex < 0 || dayIndex >= week.days.length) {
      return null;
    }

    final day = week.days[dayIndex];
    if (day.state == 'rest' || (day.sessionTypeId?.isEmpty ?? true)) {
      return null;
    }

    return _PlannedTodaySession(
      sessionTypeId: day.sessionTypeId ?? '',
      sessionTypeName: day.sessionTypeName ?? 'Sesión',
      sessionColorHex: day.sessionColorHex ?? '#5C5C5C',
    );
  }
}

class _DashboardHeader extends StatelessWidget {
  const _DashboardHeader({
    required this.user,
    required this.currentWeek,
  });

  final UserProfileModel? user;
  final PlanWeekDto? currentWeek;

  @override
  Widget build(BuildContext context) {
    final formatter = DateFormat("EEEE d 'de' MMMM", 'es');
    final formattedDate = _capitalize(formatter.format(DateTime.now()));

    return Container(
      decoration: AppDecorations.headerGradient,
      padding: const EdgeInsets.fromLTRB(
          kPaddingH, kPaddingTop, kPaddingH, kPaddingH),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(formattedDate, style: AppTypography.bodySm),
                    const SizedBox(height: kGapSm),
                    Text('Hola, ${user?.name ?? 'climber'} 👋',
                        style: AppTypography.headingLg),
                    if (currentWeek != null) ...[
                      const SizedBox(height: kGapMd),
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 12, vertical: 8),
                        decoration: BoxDecoration(
                          color: AppColors.secondary.withValues(alpha: 0.13),
                          borderRadius: BorderRadius.circular(999),
                        ),
                        child: Text(
                          'Semana ${currentWeek!.weekNumber} · Fase ${currentWeek!.phaseName}',
                          style: AppTypography.bodySm
                              .copyWith(color: AppColors.secondary),
                        ),
                      ),
                    ],
                  ],
                ),
              ),
              const SizedBox(width: kGapMd),
              _HeaderIconButton(
                icon: Icons.notifications_none_rounded,
                onTap: () {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                        content: Text('Notificaciones próximamente.')),
                  );
                },
              ),
              const SizedBox(width: kGapSm),
              InkWell(
                onTap: () => context.push('/profile'),
                borderRadius: BorderRadius.circular(999),
                child: Container(
                  width: 44,
                  height: 44,
                  decoration: const BoxDecoration(
                    color: AppColors.primary,
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.person_rounded, color: Colors.black),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  String _capitalize(String value) {
    if (value.isEmpty) {
      return value;
    }

    return value[0].toUpperCase() + value.substring(1);
  }
}

class _HeaderIconButton extends StatelessWidget {
  const _HeaderIconButton({
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
        width: 44,
        height: 44,
        decoration: BoxDecoration(
          color: AppColors.surfaceCard,
          shape: BoxShape.circle,
          border: Border.all(color: Colors.white.withValues(alpha: 0.08)),
        ),
        child: Icon(icon, color: AppColors.textPrimary),
      ),
    );
  }
}

class _TodaySessionCard extends StatelessWidget {
  const _TodaySessionCard({
    required this.session,
    required this.planWeekId,
    required this.dayOfWeek,
  });

  final SessionLogDto session;
  final int? planWeekId;
  final int dayOfWeek;

  @override
  Widget build(BuildContext context) {
    final sessionMeta =
        _sessionMeta(session.sessionTypeId, session.sessionTypeName);
    final sessionColor =
        session.sessionColorHex.toAppColor(fallback: AppColors.primary);
    final buttonLabel = session.isDone ? 'Completada ✓' : 'Iniciar sesión';
    final buttonBackground = session.isDone
        ? AppColors.success.withValues(alpha: 0.15)
        : AppColors.primary;
    final buttonForeground = session.isDone ? AppColors.success : Colors.black;

    return Container(
      decoration: AppDecorations.sessionCard(sessionColor),
      padding: const EdgeInsets.all(kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              _EnergyBadge(
                label: sessionMeta.energySystem.toUpperCase(),
                color: sessionColor,
              ),
              const Spacer(),
              _LoadIndicator(
                level: sessionMeta.loadLevel,
                color: sessionColor,
              ),
            ],
          ),
          const SizedBox(height: kGapMd),
          Text(
            session.sessionTypeName,
            style: AppTypography.headingLg.copyWith(fontSize: 22),
          ),
          const SizedBox(height: kGapSm),
          Text(
            session.notes?.isNotEmpty == true
                ? session.notes!
                : sessionMeta.description,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
            style: AppTypography.bodySm.copyWith(fontSize: 13),
          ),
          const SizedBox(height: kGapLg),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              style: ElevatedButton.styleFrom(
                backgroundColor: buttonBackground,
                foregroundColor: buttonForeground,
              ),
              onPressed: () {
                if (planWeekId == null) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                        content:
                            Text('No hay semana activa disponible todavía.')),
                  );
                  return;
                }

                context.push('/session-log/$planWeekId/$dayOfWeek');
              },
              child: Text(buttonLabel),
            ),
          ),
        ],
      ),
    );
  }
}

class _PlannedTodaySessionCard extends StatelessWidget {
  const _PlannedTodaySessionCard({
    required this.session,
    required this.planWeekId,
    required this.dayOfWeek,
  });

  final _PlannedTodaySession session;
  final int? planWeekId;
  final int dayOfWeek;

  @override
  Widget build(BuildContext context) {
    final sessionMeta =
        _sessionMeta(session.sessionTypeId, session.sessionTypeName);
    final sessionColor =
        session.sessionColorHex.toAppColor(fallback: AppColors.primary);

    return Container(
      decoration: AppDecorations.sessionCard(sessionColor),
      padding: const EdgeInsets.all(kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              _EnergyBadge(
                label: sessionMeta.energySystem.toUpperCase(),
                color: sessionColor,
              ),
              const Spacer(),
              _LoadIndicator(
                level: sessionMeta.loadLevel,
                color: sessionColor,
              ),
            ],
          ),
          const SizedBox(height: kGapMd),
          Text(
            session.sessionTypeName,
            style: AppTypography.headingLg.copyWith(fontSize: 22),
          ),
          const SizedBox(height: kGapSm),
          Text(
            sessionMeta.description,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
            style: AppTypography.bodySm.copyWith(fontSize: 13),
          ),
          const SizedBox(height: kGapLg),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () {
                if (planWeekId == null) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('No hay semana activa disponible todavía.'),
                    ),
                  );
                  return;
                }

                context.push('/session-log/$planWeekId/$dayOfWeek');
              },
              child: const Text('Iniciar sesión'),
            ),
          ),
        ],
      ),
    );
  }
}

class _RestDayCard extends StatelessWidget {
  const _RestDayCard();

  static const _messages = [
    'Hoy toca recargar energía. El descanso también entrena.',
    'Respira, movilidad ligera y vuelve más fuerte mañana.',
    'Un buen descanso hoy mejora tu rendimiento de la semana.',
  ];

  @override
  Widget build(BuildContext context) {
    final message = _messages[DateTime.now().day % _messages.length];

    return Container(
      decoration:
          AppDecorations.darkCard.copyWith(color: AppColors.surfaceVariant),
      padding: const EdgeInsets.all(kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('🏔️', style: TextStyle(fontSize: 28)),
          const SizedBox(height: kGapMd),
          const Text('Día de descanso', style: AppTypography.headingSm),
          const SizedBox(height: kGapSm),
          Text(message,
              style: AppTypography.bodyMd
                  .copyWith(color: AppColors.textSecondary)),
        ],
      ),
    );
  }
}

class _WeekStrip extends StatelessWidget {
  const _WeekStrip({
    required this.weekAsync,
    required this.labels,
  });

  final AsyncValue<PlanWeekDto> weekAsync;
  final List<String> labels;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(kPaddingH, kGapLg, kPaddingH, 0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SectionLabel('Esta semana'),
          const SizedBox(height: kGapMd),
          weekAsync.when(
            data: (week) => SizedBox(
              height: 64,
              child: ListView.separated(
                scrollDirection: Axis.horizontal,
                itemCount: week.days.length,
                separatorBuilder: (_, __) => const SizedBox(width: kGapSm),
                itemBuilder: (context, index) {
                  final day = week.days[index];
                  final dayColor = (day.sessionColorHex ?? '')
                      .toAppColor(fallback: AppColors.surfaceCard);
                  final label =
                      index < labels.length ? labels[index] : day.label;
                  return DayChip(
                    label: label,
                    state: day.state,
                    sessionColor: dayColor,
                    onTap: () => _showDayDetails(context, label, day),
                  );
                },
              ),
            ),
            loading: () => const SizedBox(
              height: 64,
              child: Center(child: CircularProgressIndicator()),
            ),
            error: (error, _) => _InlineErrorText(message: error.toString()),
          ),
        ],
      ),
    );
  }

  void _showDayDetails(BuildContext context, String label, DayEntryDto day) {
    showModalBottomSheet<void>(
      context: context,
      backgroundColor: AppColors.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(kRadiusXl)),
      ),
      builder: (_) {
        final sessionColor = (day.sessionColorHex ?? '')
            .toAppColor(fallback: AppColors.surfaceCard);
        return Padding(
          padding: const EdgeInsets.all(kPaddingH),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label, style: AppTypography.bodySm),
              const SizedBox(height: kGapSm),
              Text(
                day.sessionTypeName ??
                    (day.state == 'rest' ? 'Descanso' : 'Sesión pendiente'),
                style: AppTypography.headingSm.copyWith(
                  color: day.state == 'rest'
                      ? AppColors.textPrimary
                      : sessionColor,
                ),
              ),
              const SizedBox(height: kGapSm),
              Text(
                _dayStateMessage(day.state),
                style: AppTypography.bodyMd
                    .copyWith(color: AppColors.textSecondary),
              ),
              const SizedBox(height: kGapLg),
            ],
          ),
        );
      },
    );
  }

  String _dayStateMessage(String state) {
    return switch (state) {
      'completed' => 'Sesión completada correctamente.',
      'today' => 'Hoy tienes actividad programada.',
      'pending' => 'La sesión sigue pendiente para esta semana.',
      'failed' => 'Esta sesión quedó pendiente en una fecha pasada.',
      _ => 'Día de recuperación y descanso.',
    };
  }
}

class _QuickStats extends StatelessWidget {
  const _QuickStats({
    required this.summaryAsync,
  });

  final AsyncValue<StatsSummaryDto> summaryAsync;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(kPaddingH, kGapLg, kPaddingH, 0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SectionLabel('Stats rápidas'),
          const SizedBox(height: kGapMd),
          summaryAsync.when(
            data: (summary) => Row(
              children: [
                Expanded(
                  child: _QuickStatCard(
                    label: 'Sesiones',
                    value: '${summary.sessionsThisWeek}',
                    icon: Icons.bolt_rounded,
                    iconColor: AppColors.secondary,
                  ),
                ),
                const SizedBox(width: kGapSm),
                Expanded(
                  child: _QuickStatCard(
                    label: 'Racha',
                    value: '${summary.currentStreak}',
                    icon: Icons.local_fire_department_rounded,
                    iconColor: AppColors.primary,
                  ),
                ),
                const SizedBox(width: kGapSm),
                Expanded(
                  child: _QuickStatCard(
                    label: 'RPE medio',
                    value: summary.rpeAverage.toStringAsFixed(1),
                    icon: Icons.insights_rounded,
                    iconColor: AppColors.tertiary,
                  ),
                ),
              ],
            ),
            loading: () => const SizedBox(
              height: 128,
              child: Center(child: CircularProgressIndicator()),
            ),
            error: (error, _) => _ErrorCard(message: error.toString()),
          ),
        ],
      ),
    );
  }
}

class _QuickStatCard extends StatelessWidget {
  const _QuickStatCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.iconColor,
  });

  final String label;
  final String value;
  final IconData icon;
  final Color iconColor;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: AppDecorations.darkCard,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SectionLabel(label),
          const SizedBox(height: kGapSm),
          Row(
            children: [
              Expanded(
                child: Text(value,
                    style: AppTypography.metricLg.copyWith(fontSize: 22)),
              ),
              Icon(icon, size: 20, color: iconColor),
            ],
          ),
        ],
      ),
    );
  }
}

class _EnergyBadge extends StatelessWidget {
  const _EnergyBadge({
    required this.label,
    required this.color,
  });

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.13),
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label,
        style: AppTypography.labelCaps.copyWith(color: color),
      ),
    );
  }
}

class _LoadIndicator extends StatelessWidget {
  const _LoadIndicator({
    required this.level,
    required this.color,
  });

  final int level;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: List.generate(4, (index) {
        final isActive = index < level;
        return Container(
          width: 8,
          height: 8,
          margin: EdgeInsets.only(left: index == 0 ? 0 : 4),
          decoration: BoxDecoration(
            color: isActive ? color : Colors.white.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(999),
          ),
        );
      }),
    );
  }
}

class _SessionCardSkeleton extends StatelessWidget {
  const _SessionCardSkeleton();

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 220,
      decoration: AppDecorations.darkCard,
      child: const Center(child: CircularProgressIndicator()),
    );
  }
}

class _ErrorCard extends StatelessWidget {
  const _ErrorCard({
    required this.message,
  });

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: AppDecorations.darkCard,
      padding: const EdgeInsets.all(kGapLg),
      child: Text(
        message.replaceFirst('Exception: ', ''),
        style: AppTypography.bodyMd.copyWith(color: AppColors.error),
      ),
    );
  }
}

class _InlineErrorText extends StatelessWidget {
  const _InlineErrorText({
    required this.message,
  });

  final String message;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: kGapSm),
      child: Text(
        message.replaceFirst('Exception: ', ''),
        style: AppTypography.bodySm.copyWith(color: AppColors.error),
      ),
    );
  }
}

_SessionMeta _sessionMeta(String sessionTypeId, String sessionTypeName) {
  switch (sessionTypeId) {
    case 'arc':
      return const _SessionMeta('Aeróbico', 1,
          'Sesión continua de volumen suave para construir base aeróbica.');
    case 'hangboard':
      return const _SessionMeta('Aláctico', 4,
          'Trabajo de fuerza máxima en agarres con descansos largos.');
    case 'campus_hang':
    case 'campus_limit':
      return const _SessionMeta(
          'Aláctico', 4, 'Estímulo explosivo y de potencia en campus board.');
    case 'linked':
      return const _SessionMeta('Láctico', 2,
          'Bloques enlazados para tolerancia al esfuerzo sostenido.');
    case 'outdoor':
      return const _SessionMeta('Aeróbico', 3,
          'Jornada de roca con volumen técnico y control de fatiga.');
    case 'rest':
      return const _SessionMeta('Recuperación', 0,
          'Recuperación activa y preparación para la siguiente carga.');
    case 'boulder':
    case 'limit':
      return const _SessionMeta('Aláctico', 3,
          'Problemas intensos enfocados en fuerza y coordinación.');
    default:
      return _SessionMeta(
          'Sesión', 2, 'Actividad planificada: $sessionTypeName.');
  }
}

class _SessionMeta {
  const _SessionMeta(this.energySystem, this.loadLevel, this.description);

  final String energySystem;
  final int loadLevel;
  final String description;
}

class _PlannedTodaySession {
  const _PlannedTodaySession({
    required this.sessionTypeId,
    required this.sessionTypeName,
    required this.sessionColorHex,
  });

  final String sessionTypeId;
  final String sessionTypeName;
  final String sessionColorHex;
}
