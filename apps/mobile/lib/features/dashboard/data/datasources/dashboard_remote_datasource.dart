import 'package:dio/dio.dart';

import '../../../plan/data/models/plan_models.dart';
import '../models/dashboard_models.dart';

class DashboardRemoteDatasource {
  DashboardRemoteDatasource(this._dio);

  final Dio _dio;

  Future<SessionLogDto?> getTodaySession() async {
    try {
      final response =
          await _dio.get<Map<String, dynamic>>('/api/v1/session-logs/today');
      final data = response.data;
      if (data == null || data.isEmpty) {
        return null;
      }

      return SessionLogDto.fromJson(data);
    } on DioException catch (error) {
      if (error.response?.statusCode == 404) {
        return null;
      }

      rethrow;
    }
  }

  Future<PlanWeekDto> getCurrentWeek() async {
    final response = await _dio
        .get<Map<String, dynamic>>('/api/v1/plans/active/weeks/current');
    return PlanWeekDto.fromJson(response.data ?? <String, dynamic>{});
  }

  Future<StatsSummaryDto> getStatsSummary() async {
    final response =
        await _dio.get<Map<String, dynamic>>('/api/v1/stats/summary');
    return StatsSummaryDto.fromJson(response.data ?? <String, dynamic>{});
  }
}
