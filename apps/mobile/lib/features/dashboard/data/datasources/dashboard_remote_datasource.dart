import 'package:dio/dio.dart';

import '../models/plan_week_model.dart';
import '../models/stats_summary_model.dart';
import '../models/today_session_model.dart';
import '../models/user_plan_detail_model.dart';

class DashboardRemoteDatasource {
  DashboardRemoteDatasource(this._dio);

  final Dio _dio;

  Future<UserPlanDetailModel?> getActivePlan() async {
    final response = await _dio.get<Map<String, dynamic>>('/plans/active');
    final data = response.data;
    if (data == null || data.isEmpty) {
      return null;
    }

    return UserPlanDetailModel.fromJson(data);
  }

  Future<PlanWeekModel?> getCurrentWeek() async {
    final response = await _dio.get<Map<String, dynamic>>('/plans/active/weeks/current');
    final data = response.data;
    if (data == null || data.isEmpty) {
      return null;
    }

    return PlanWeekModel.fromJson(data);
  }

  Future<TodaySessionModel?> getTodaySession() async {
    final response = await _dio.get<dynamic>('/session-logs/today');
    final data = response.data;
    if (data == null) {
      return null;
    }

    if (data is Map<String, dynamic>) {
      return TodaySessionModel.fromJson(data);
    }

    return null;
  }

  Future<StatsSummaryModel> getStatsSummary() async {
    final response = await _dio.get<Map<String, dynamic>>('/stats/summary');
    return StatsSummaryModel.fromJson(response.data ?? <String, dynamic>{});
  }
}
