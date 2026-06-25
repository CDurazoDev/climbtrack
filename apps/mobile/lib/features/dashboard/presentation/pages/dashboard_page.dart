import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_typography.dart';
import '../../../auth/data/models/auth_tokens_model.dart';
import '../../../auth/presentation/providers/auth_state.dart';
import '../../data/models/dashboard_snapshot_model.dart';
import '../../data/models/day_entry_model.dart';
import '../../data/models/plan_week_model.dart';
import '../../data/models/stats_summary_model.dart';
import '../../data/models/today_session_model.dart';
import '../../data/models/user_plan_detail_model.dart';
import '../providers/dashboard_providers.dart';
import '../widgets/dashboard_day_chip.dart';
import '../widgets/dashboard_section_card.dart';
import '../widgets/dashboard_stat_tile.dart';

class DashboardPage extends ConsumerWidget {
  const DashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authStateProvider).valueOrNull;
    final user = authState?.maybeWhen<UserProfileModel?>(
      authenticated: (value) => value,
      orElse: () => null,
    );
    final dashboardAsync = ref.watch(dashboardProvider);

    return Scaffold(
      body: SafeArea(
        child: RefreshIndicator(
          color: AppColors.primary,
          backgroundColor: AppColors.surfaceVariant,
          onRefresh: () => ref.read(dashboardProvider.notifier).refreshDashboard(),
          child: dashboardAsync.when(
            data: (snapshot) => _DashboardView(
              user: user,
              snapshot: snapshot,
              onLogout: () => ref.read(authStateProvider.notifier).logout(),
            ),
            loading: () => const _DashboardLoadingView(),
            error: (error, _) => _DashboardErrorView(
              message: error.toString(),
              onRetry: () => ref.read(dashboardProvider.notifier).refreshDashboard(),
              onLogout: () => ref.read(authStateProvider.notifier).logout(),
            ),
          ),
        ),
      ),
    );
  }
}

class _DashboardView extends StatelessWidget {
  const _DashboardView({
    required this.user,
    required this.snapshot,
    required this.onLogout,
  });

  final UserProfileModel? user;
  final DashboardSnapshotModel snapshot;
  final VoidCallback onLogout;

  @override
  Widget build(BuildContext context) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(kPaddingH, kPaddingTop, kPaddingH, kGapLg),
      children: [
        _DashboardHeader(
          user: user,
          onLogout: onLogout,
        ),
        const SizedBox(height: kGapLg),
        _PlanOverviewCard(
          activePlan: snapshot.activePlan,
          currentWeek: snapshot.currentWeek,
        ),
        const SizedBox(height: kGapLg),
        _TodaySessionCard(session: snapshot.todaySession),
        const SizedBox(height: kGapLg),
        _QuickStatsSection(summary: snapshot.statsSummary),
      ],
    );
  }
}

class _DashboardLoadingView extends StatelessWidget {
  const _DashboardLoadingView();

  @override
  Widget build(BuildContext context) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(kPaddingH, kPaddingTop, kPaddingH, kGapLg),
      children: const [
        SizedBox(height: 160),
        Center(
          child: CircularProgressIndicator(color: AppColors.primary),
        ),
      ],
    );
  }
}

class _DashboardErrorView extends StatelessWidget {
  const _DashboardErrorView({
    required this.message,
    required this.onRetry,
    required this.onLogout,
  });

  final String message;
  final Future<void> Function() onRetry;
  final VoidCallback onLogout;

  @override
  Widget build(BuildContext context) {
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.fromLTRB(kPaddingH, kPaddingTop, kPaddingH, kGapLg),
      children: [
        _DashboardHeader(
          user: null,
          onLogout: onLogout,
        ),
        const SizedBox(height: kGapLg),
        DashboardSectionCard(
          title: 'Dashboard unavailable',
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                message,
                style: AppTypography.bodyMd,
              ),
              const SizedBox(height: kGapMd),
              ElevatedButton(
                onPressed: () => onRetry(),
                child: const Text('Try again'),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _DashboardHeader extends StatelessWidget {
  const _DashboardHeader({
    required this.user,
    required this.onLogout,
  });

  final UserProfileModel? user;
  final VoidCallback onLogout;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapLg),
      decoration: BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(kRadiusXl),
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppColors.surfaceVariant,
            AppColors.primary.withValues(alpha: 0.08),
          ],
        ),
        border: Border.all(color: Colors.white10),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Expanded(
                child: Text(
                  'ClimbTrack',
                  style: AppTypography.labelCaps,
                ),
              ),
              TextButton(
                onPressed: onLogout,
                child: const Text('Sign out'),
              ),
            ],
          ),
          const SizedBox(height: kGapSm),
          Text(
            user == null ? 'Welcome back' : 'Welcome back, ${user!.name}',
            style: AppTypography.headingLg,
          ),
          const SizedBox(height: kGapSm),
          Text(
            'Your current plan, today\'s session, and quick stats in one place.',
            style: AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
          ),
        ],
      ),
    );
  }
}

class _PlanOverviewCard extends StatelessWidget {
  const _PlanOverviewCard({
    required this.activePlan,
    required this.currentWeek,
  });

  final UserPlanDetailModel? activePlan;
  final PlanWeekModel? currentWeek;

  @override
  Widget build(BuildContext context) {
    if (activePlan == null) {
      return const DashboardSectionCard(
        title: 'Active plan',
        child: Text(
          'No active plan is available yet.',
          style: AppTypography.bodyMd,
        ),
      );
    }

    final phaseColor = _parseColor(currentWeek?.phaseColorHex) ?? AppColors.tertiary;

    return DashboardSectionCard(
      title: 'Active plan',
      trailing: currentWeek == null
          ? null
          : Container(
              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
              decoration: BoxDecoration(
                color: phaseColor.withValues(alpha: 0.16),
                borderRadius: BorderRadius.circular(kRadiusSm),
              ),
              child: Text(
                'Week ${currentWeek!.weekNumber}',
                style: AppTypography.bodySm.copyWith(color: phaseColor),
              ),
            ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            activePlan!.name,
            style: AppTypography.headingSm,
          ),
          const SizedBox(height: kGapSm),
          Wrap(
            spacing: kGapSm,
            runSpacing: kGapSm,
            children: [
              _TagChip(label: _titleize(activePlan!.trainingTypeCode)),
              _TagChip(label: _titleize(activePlan!.difficultyLevelCode)),
              _TagChip(label: '${activePlan!.weeks.length} weeks'),
              if (currentWeek?.isDeload == true) const _TagChip(label: 'Deload'),
            ],
          ),
          if (currentWeek != null) ...[
            const SizedBox(height: kGapLg),
            Text(
              currentWeek!.phaseName,
              style: AppTypography.bodyMd.copyWith(
                color: phaseColor,
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: kGapSm),
            ClipRRect(
              borderRadius: BorderRadius.circular(kRadiusSm),
              child: LinearProgressIndicator(
                minHeight: 8,
                value: currentWeek!.normalizedProgress,
                backgroundColor: AppColors.surfaceCard,
                valueColor: AlwaysStoppedAnimation<Color>(phaseColor),
              ),
            ),
            const SizedBox(height: kGapSm),
            Text(
              '${currentWeek!.progressPercent}% completed',
              style: AppTypography.bodySm,
            ),
            const SizedBox(height: kGapLg),
            Wrap(
              spacing: kGapSm,
              runSpacing: kGapSm,
              children: currentWeek!.days
                  .map((entry) => DashboardDayChip(entry: entry))
                  .toList(growable: false),
            ),
            const SizedBox(height: kGapMd),
            Text(
              _resolveTodayHint(currentWeek!.days),
              style: AppTypography.bodySm,
            ),
          ],
        ],
      ),
    );
  }

  static String _resolveTodayHint(List<DayEntryModel> days) {
    final entries = days.where((entry) => entry.state == 'today');
    if (entries.isEmpty) {
      return 'Pull down to refresh the latest week progress.';
    }

    final todayEntry = entries.first;
    if ((todayEntry.sessionTypeName ?? '').isEmpty) {
      return 'Today is marked as a rest day.';
    }

    return 'Today: ${todayEntry.sessionTypeName}';
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

  String _titleize(String value) {
    return value
        .split(RegExp(r'[_\s-]+'))
        .where((part) => part.isNotEmpty)
        .map((part) => '${part[0].toUpperCase()}${part.substring(1)}')
        .join(' ');
  }
}

class _TodaySessionCard extends StatelessWidget {
  const _TodaySessionCard({
    required this.session,
  });

  final TodaySessionModel? session;

  @override
  Widget build(BuildContext context) {
    if (session == null) {
      return const DashboardSectionCard(
        title: 'Today\'s session',
        child: Text(
          'No session log is available for today yet.',
          style: AppTypography.bodyMd,
        ),
      );
    }

    final sessionColor = _parseColor(session!.sessionColorHex) ?? AppColors.secondary;

    return Container(
      decoration: BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(kRadiusLg),
        border: Border.all(color: sessionColor.withValues(alpha: 0.26)),
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppColors.surfaceVariant,
            sessionColor.withValues(alpha: 0.08),
          ],
        ),
      ),
      padding: const EdgeInsets.all(kGapMd),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Expanded(
                child: Text(
                  'Today\'s session',
                  style: AppTypography.headingSm,
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                decoration: BoxDecoration(
                  color: session!.isDone
                      ? AppColors.success.withValues(alpha: 0.16)
                      : AppColors.primary.withValues(alpha: 0.16),
                  borderRadius: BorderRadius.circular(kRadiusSm),
                ),
                child: Text(
                  session!.isDone ? 'Completed' : 'Pending',
                  style: AppTypography.bodySm.copyWith(
                    color: session!.isDone ? AppColors.success : AppColors.primary,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: kGapMd),
          Text(
            session!.sessionTypeName,
            style: AppTypography.headingSm.copyWith(color: sessionColor),
          ),
          const SizedBox(height: kGapSm),
          Wrap(
            spacing: kGapSm,
            runSpacing: kGapSm,
            children: [
              if (session!.durationMin != null) _TagChip(label: '${session!.durationMin} min'),
              if (session!.rpe != null) _TagChip(label: 'RPE ${session!.rpe}'),
              _TagChip(label: _formatDate(session!.logDate)),
            ],
          ),
          if (session!.notes != null && session!.notes!.trim().isNotEmpty) ...[
            const SizedBox(height: kGapMd),
            Text(
              session!.notes!,
              style: AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
            ),
          ],
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

  String _formatDate(DateTime value) {
    final month = value.month.toString().padLeft(2, '0');
    final day = value.day.toString().padLeft(2, '0');
    return '$month/$day';
  }
}

class _QuickStatsSection extends StatelessWidget {
  const _QuickStatsSection({
    required this.summary,
  });

  final StatsSummaryModel summary;

  @override
  Widget build(BuildContext context) {
    return DashboardSectionCard(
      title: 'Quick stats',
      child: GridView.count(
        crossAxisCount: 2,
        mainAxisSpacing: kGapMd,
        crossAxisSpacing: kGapMd,
        shrinkWrap: true,
        physics: const NeverScrollableScrollPhysics(),
        childAspectRatio: 1.25,
        children: [
          DashboardStatTile(
            label: 'Total sessions',
            value: '${summary.totalSessions}',
            accentColor: AppColors.primary,
          ),
          DashboardStatTile(
            label: 'Current streak',
            value: '${summary.currentStreak}',
            accentColor: AppColors.secondary,
          ),
          DashboardStatTile(
            label: 'Best streak',
            value: '${summary.maxStreak}',
            accentColor: AppColors.success,
          ),
          DashboardStatTile(
            label: 'Average RPE',
            value: _formatRpe(summary.rpeAverage),
            accentColor: AppColors.rpeColor(summary.rpeAverage.round()),
          ),
          DashboardStatTile(
            label: 'This week',
            value: '${summary.sessionsThisWeek}',
            accentColor: AppColors.tertiary,
          ),
          DashboardStatTile(
            label: 'Weeks completed',
            value: '${summary.weeksCompleted}',
            accentColor: AppColors.phasePerformance,
          ),
        ],
      ),
    );
  }

  String _formatRpe(double value) {
    if (value == value.roundToDouble()) {
      return value.toStringAsFixed(0);
    }

    return value.toStringAsFixed(1);
  }
}

class _TagChip extends StatelessWidget {
  const _TagChip({
    required this.label,
  });

  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.surfaceCard,
        borderRadius: BorderRadius.circular(kRadiusSm),
        border: Border.all(color: Colors.white10),
      ),
      child: Text(
        label,
        style: AppTypography.bodySm,
      ),
    );
  }
}
