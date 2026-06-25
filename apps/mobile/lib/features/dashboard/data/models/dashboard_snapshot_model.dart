import 'plan_week_model.dart';
import 'stats_summary_model.dart';
import 'today_session_model.dart';
import 'user_plan_detail_model.dart';

class DashboardSnapshotModel {
  const DashboardSnapshotModel({
    required this.activePlan,
    required this.currentWeek,
    required this.todaySession,
    required this.statsSummary,
  });

  final UserPlanDetailModel? activePlan;
  final PlanWeekModel? currentWeek;
  final TodaySessionModel? todaySession;
  final StatsSummaryModel statsSummary;
}
