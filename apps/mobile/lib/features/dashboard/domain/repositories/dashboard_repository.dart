import 'package:dartz/dartz.dart';

import '../../../../core/error/failure.dart';
import '../../data/models/dashboard_snapshot_model.dart';

abstract class DashboardRepository {
  Future<Either<Failure, DashboardSnapshotModel>> loadDashboard();
}
