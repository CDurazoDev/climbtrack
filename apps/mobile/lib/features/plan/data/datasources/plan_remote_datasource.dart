import 'package:dio/dio.dart';

import '../models/plan_models.dart';

class PlanRemoteDatasource {
  PlanRemoteDatasource(this._dio);

  final Dio _dio;

  Future<UserPlanDetailDto> getActivePlan() async {
    final response =
        await _dio.get<Map<String, dynamic>>('/api/v1/plans/active');
    return UserPlanDetailDto.fromJson(response.data ?? <String, dynamic>{});
  }

  Future<PlanWeekDto> getPlanWeekDetail(int planId, int weekNumber) async {
    final response = await _dio
        .get<Map<String, dynamic>>('/api/v1/plans/$planId/weeks/$weekNumber');
    return PlanWeekDto.fromJson(response.data ?? <String, dynamic>{});
  }
}
