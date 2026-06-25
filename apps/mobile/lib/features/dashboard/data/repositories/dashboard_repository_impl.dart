import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';

import '../../../../core/error/failure.dart';
import '../../domain/repositories/dashboard_repository.dart';
import '../datasources/dashboard_remote_datasource.dart';
import '../models/dashboard_snapshot_model.dart';
import '../models/plan_week_model.dart';
import '../models/stats_summary_model.dart';
import '../models/today_session_model.dart';
import '../models/user_plan_detail_model.dart';

class DashboardRepositoryImpl implements DashboardRepository {
  DashboardRepositoryImpl(this._remoteDatasource);

  final DashboardRemoteDatasource _remoteDatasource;

  @override
  Future<Either<Failure, DashboardSnapshotModel>> loadDashboard() async {
    try {
      final activePlanFuture = _getNullable(
        _remoteDatasource.getActivePlan,
        null,
      );
      final currentWeekFuture = _getNullable(
        _remoteDatasource.getCurrentWeek,
        null,
      );
      final todaySessionFuture = _getNullable(
        _remoteDatasource.getTodaySession,
        null,
      );
      final statsSummaryFuture = _remoteDatasource.getStatsSummary();

      final results = await Future.wait<dynamic>([
        activePlanFuture,
        currentWeekFuture,
        todaySessionFuture,
        statsSummaryFuture,
      ]);

      return Right(
        DashboardSnapshotModel(
          activePlan: results[0] as UserPlanDetailModel?,
          currentWeek: results[1] as PlanWeekModel?,
          todaySession: results[2] as TodaySessionModel?,
          statsSummary: results[3] as StatsSummaryModel,
        ),
      );
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  Future<T?> _getNullable<T>(
    Future<T?> Function() request,
    T? fallback,
  ) async {
    try {
      return await request();
    } on DioException catch (error) {
      if (error.response?.statusCode == 404) {
        return fallback;
      }

      rethrow;
    }
  }

  String _extractMessage(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic>) {
      final message = data['detail'] ?? data['message'] ?? data['title'];
      if (message is String && message.isNotEmpty) {
        return message;
      }
    }

    return error.message ?? 'Unknown error';
  }
}
