import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';

import '../../../../core/error/failure.dart';
import '../../../dashboard/data/models/dashboard_models.dart';
import '../../../plan/data/datasources/plan_remote_datasource.dart';
import '../../../plan/data/models/plan_models.dart';
import '../../domain/entities/session_metrics.dart';
import '../../domain/repositories/session_log_repository.dart';
import '../datasources/session_log_remote_datasource.dart';
import '../models/session_log_models.dart';

class SessionLogRepositoryImpl implements SessionLogRepository {
  SessionLogRepositoryImpl(this._remote, this._planRemote);

  final SessionLogRemoteDatasource _remote;
  final PlanRemoteDatasource _planRemote;

  @override
  Future<Either<Failure, DaySessionDetails>> getDaySession({
    required int weekId,
    required int dayOfWeek,
  }) async {
    try {
      final activePlan = await _planRemote.getActivePlan();
      final week = _findWeek(activePlan, weekId);
      if (week == null) {
        return const Left(
          ValidationFailure('The selected training week was not found.'),
        );
      }

      if (dayOfWeek < 0 || dayOfWeek >= week.days.length) {
        return const Left(
          ValidationFailure('The selected day is not valid for this week.'),
        );
      }

      final dayEntry = week.days[dayOfWeek];
      final sessionTypeCode = dayEntry.sessionTypeId;
      if (dayEntry.state == 'rest' ||
          sessionTypeCode == null ||
          sessionTypeCode.isEmpty) {
        return const Left(
          ValidationFailure('No session is scheduled for this day.'),
        );
      }

      final sessionTypes = await _remote.getSessionTypes();
      final sessionType = sessionTypes.cast<SessionTypeDto?>().firstWhere(
            (item) => item?.code == sessionTypeCode,
            orElse: () => null,
          );

      if (sessionType == null) {
        return const Left(
          ServerFailure('The session type configuration is not available.'),
        );
      }

      final logDate =
          _resolveLogDate(activePlan.startDate, week.weekNumber, dayOfWeek);
      final weekLogs = await _remote.getWeekSessionLogs(weekId);
      var sessionLog = _findLogForDate(weekLogs, logDate, sessionType.code);

      sessionLog ??= await _remote.createSessionLog(
        CreateSessionLogRequest(
          userPlanWeekId: weekId,
          sessionTypeId: sessionType.code,
          logDate: logDate,
          dayOfWeek: dayOfWeek,
        ),
      );

      return Right(
        DaySessionDetails(
          weekId: weekId,
          dayOfWeek: dayOfWeek,
          logDate: logDate,
          dayEntry: dayEntry,
          sessionType: sessionType,
          sessionLog: sessionLog,
        ),
      );
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  @override
  Future<Either<Failure, SessionLogDto>> completeSession({
    required int sessionLogId,
    required int rpe,
    required int durationMin,
    required String notes,
    required List<MetricInput> metrics,
  }) async {
    try {
      final result = await _remote.completeSession(
        id: sessionLogId,
        request: CompleteSessionRequest(
          rpe: rpe,
          durationMin: durationMin,
          notes: notes.isEmpty ? null : notes,
          metrics: metrics,
        ),
      );

      return Right(result);
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  PlanWeekDto? _findWeek(UserPlanDetailDto plan, int weekId) {
    for (final week in plan.weeks) {
      if (week.id == weekId) {
        return week;
      }
    }

    return null;
  }

  SessionLogDto? _findLogForDate(
    List<SessionLogDto> logs,
    DateTime targetDate,
    String sessionTypeCode,
  ) {
    for (final log in logs.reversed) {
      if (_isSameDate(log.logDate, targetDate) &&
          log.sessionTypeId == sessionTypeCode) {
        return log;
      }
    }

    return null;
  }

  DateTime _resolveLogDate(
    DateTime planStartDate,
    int weekNumber,
    int dayOfWeek,
  ) {
    final dayOffset = ((weekNumber - 1) * 7) + dayOfWeek;
    final date = DateTime(
      planStartDate.year,
      planStartDate.month,
      planStartDate.day,
    ).add(Duration(days: dayOffset));

    return DateTime(date.year, date.month, date.day);
  }

  bool _isSameDate(DateTime left, DateTime right) {
    return left.year == right.year &&
        left.month == right.month &&
        left.day == right.day;
  }

  String _extractMessage(DioException error) {
    final data = error.response?.data;
    if (data is Map<String, dynamic>) {
      final message = data['detail'] ?? data['message'] ?? data['title'];
      if (message is String && message.isNotEmpty) {
        return message;
      }
    }

    return error.message ?? 'Unexpected network error.';
  }
}
