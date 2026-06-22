import 'package:dartz/dartz.dart';

import '../../../../core/error/failure.dart';
import '../../../dashboard/data/models/dashboard_models.dart';
import '../../data/models/session_log_models.dart';
import '../entities/session_metrics.dart';

abstract class SessionLogRepository {
  Future<Either<Failure, DaySessionDetails>> getDaySession({
    required int weekId,
    required int dayOfWeek,
  });

  Future<Either<Failure, SessionLogDto>> completeSession({
    required int sessionLogId,
    required int rpe,
    required int durationMin,
    required String notes,
    required List<MetricInput> metrics,
  });
}
