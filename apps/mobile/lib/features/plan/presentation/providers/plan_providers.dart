import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/network/dio_provider.dart';
import '../../data/datasources/plan_remote_datasource.dart';
import '../../data/models/plan_models.dart';

final planRemoteDatasourceProvider = Provider<PlanRemoteDatasource>((ref) {
  return PlanRemoteDatasource(ref.read(dioProvider));
});

final activePlanProvider =
    FutureProvider.autoDispose<UserPlanDetailDto>((ref) async {
  try {
    return await ref.read(planRemoteDatasourceProvider).getActivePlan();
  } on DioException catch (error) {
    throw Exception(_extractApiMessage(error));
  }
});

final planWeekDetailProvider = FutureProvider.autoDispose
    .family<PlanWeekDto, ({int planId, int weekNumber})>((ref, args) async {
  try {
    return await ref
        .read(planRemoteDatasourceProvider)
        .getPlanWeekDetail(args.planId, args.weekNumber);
  } on DioException catch (error) {
    throw Exception(_extractApiMessage(error));
  }
});

String _extractApiMessage(DioException error) {
  final data = error.response?.data;
  if (data is Map<String, dynamic>) {
    final message = data['detail'] ?? data['message'] ?? data['title'];
    if (message is String && message.isNotEmpty) {
      return message;
    }
  }

  return error.message ?? 'Unexpected network error.';
}
