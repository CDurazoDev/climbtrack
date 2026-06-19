import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../features/auth/presentation/providers/session_providers.dart';
import '../config/app_config.dart';
import '../storage/secure_storage_provider.dart';

final dioProvider = Provider<Dio>((ref) {
  if (AppConfig.apiBaseUrl.isEmpty) {
    throw StateError('API_BASE_URL is required. Pass it via --dart-define.');
  }

  final storage = ref.read(secureStorageProvider);
  final sessionRefreshCoordinator = ref.read(sessionRefreshCoordinatorProvider);
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 30),
      sendTimeout: const Duration(seconds: 15),
      headers: const {'Accept': 'application/json'},
    ),
  );

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        final skipAuth = options.extra['skipAuth'] == true;
        if (!skipAuth) {
          final accessToken = await storage.readAccessToken();
          if (accessToken != null && accessToken.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $accessToken';
          }
        }
        handler.next(options);
      },
      onError: (error, handler) async {
        final requestOptions = error.requestOptions;
        final skipAuth = requestOptions.extra['skipAuth'] == true;
        final alreadyRetried = requestOptions.extra['authRetry'] == true;
        final isUnauthorized = error.response?.statusCode == 401;

        if (skipAuth || alreadyRetried || !isUnauthorized) {
          handler.next(error);
          return;
        }

        try {
          final session = await sessionRefreshCoordinator.refreshIfPossible();
          if (session == null) {
            handler.next(error);
            return;
          }

          final headers = Map<String, dynamic>.from(requestOptions.headers);
          headers['Authorization'] = 'Bearer ${session.accessToken}';

          final extra = Map<String, dynamic>.from(requestOptions.extra);
          extra['authRetry'] = true;

          final response = await dio.fetch<dynamic>(
            requestOptions.copyWith(
              headers: headers,
              extra: extra,
            ),
          );

          handler.resolve(response);
        } on DioException catch (refreshError) {
          handler.next(refreshError);
        } catch (_) {
          handler.next(error);
        }
      },
    ),
  );

  if (kDebugMode || AppConfig.enableDebugLogging) {
    dio.interceptors.add(
      LogInterceptor(
        requestBody: true,
        responseBody: true,
        error: true,
        requestHeader: false,
        responseHeader: false,
      ),
    );
  }

  return dio;
});
