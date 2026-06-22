import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/network/dio_provider.dart';
import '../../data/datasources/dashboard_remote_datasource.dart';
import '../../data/models/dashboard_models.dart';
import '../../../plan/data/models/plan_models.dart';

final dashboardRemoteDatasourceProvider =
    Provider<DashboardRemoteDatasource>((ref) {
  return DashboardRemoteDatasource(ref.read(dioProvider));
});

final todaySessionProvider =
    FutureProvider.autoDispose<SessionLogDto?>((ref) async {
  try {
    return await ref.read(dashboardRemoteDatasourceProvider).getTodaySession();
  } on DioException catch (error) {
    throw Exception(_extractApiMessage(error));
  }
});

final currentWeekProvider =
    FutureProvider.autoDispose<PlanWeekDto>((ref) async {
  try {
    return await ref.read(dashboardRemoteDatasourceProvider).getCurrentWeek();
  } on DioException catch (error) {
    throw Exception(_extractApiMessage(error));
  }
});

final statsSummaryProvider =
    FutureProvider.autoDispose<StatsSummaryDto>((ref) async {
  try {
    return await ref.read(dashboardRemoteDatasourceProvider).getStatsSummary();
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
