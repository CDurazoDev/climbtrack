import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/config/app_config.dart';
import '../../../../core/storage/secure_storage_provider.dart';
import '../../data/services/session_refresh_coordinator.dart';

final sessionVersionProvider = StateProvider<int>((ref) => 0);

final sessionRefreshCoordinatorProvider = Provider<SessionRefreshCoordinator>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 30),
      sendTimeout: const Duration(seconds: 15),
      headers: const {'Accept': 'application/json'},
    ),
  );

  return SessionRefreshCoordinator(
    dio: dio,
    storage: ref.read(secureStorageProvider),
    onSessionInvalidated: () {
      ref.read(sessionVersionProvider.notifier).state++;
    },
  );
});
