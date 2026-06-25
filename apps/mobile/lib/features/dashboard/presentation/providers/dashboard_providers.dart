import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/network/dio_provider.dart';
import '../../data/datasources/dashboard_remote_datasource.dart';
import '../../data/models/dashboard_snapshot_model.dart';
import '../../data/repositories/dashboard_repository_impl.dart';
import '../../domain/repositories/dashboard_repository.dart';

final dashboardRemoteDatasourceProvider = Provider<DashboardRemoteDatasource>((ref) {
  return DashboardRemoteDatasource(ref.read(dioProvider));
});

final dashboardRepositoryProvider = Provider<DashboardRepository>((ref) {
  return DashboardRepositoryImpl(ref.read(dashboardRemoteDatasourceProvider));
});

class DashboardNotifier extends AsyncNotifier<DashboardSnapshotModel> {
  @override
  Future<DashboardSnapshotModel> build() async {
    return _loadDashboard();
  }

  Future<void> refreshDashboard() async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(_loadDashboard);
  }

  Future<DashboardSnapshotModel> _loadDashboard() async {
    final result = await ref.read(dashboardRepositoryProvider).loadDashboard();
    return result.fold(
      (failure) => throw DashboardLoadException(failure.message),
      (snapshot) => snapshot,
    );
  }
}

final dashboardProvider = AsyncNotifierProvider<DashboardNotifier, DashboardSnapshotModel>(
  DashboardNotifier.new,
);

class DashboardLoadException implements Exception {
  const DashboardLoadException(this.message);

  final String message;

  @override
  String toString() => message;
}
