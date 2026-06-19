import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../../../core/storage/secure_storage_provider.dart';
import '../models/auth_tokens_model.dart';

class SessionRefreshCoordinator {
  SessionRefreshCoordinator({
    required Dio dio,
    required FlutterSecureStorage storage,
    required VoidCallback onSessionInvalidated,
  })  : _dio = dio,
        _storage = storage,
        _onSessionInvalidated = onSessionInvalidated;

  final Dio _dio;
  final FlutterSecureStorage _storage;
  final VoidCallback _onSessionInvalidated;

  Completer<AuthTokensModel?>? _inFlightRefresh;

  Future<AuthTokensModel?> refreshIfPossible() async {
    final existingRefresh = _inFlightRefresh;
    if (existingRefresh != null) {
      return existingRefresh.future;
    }

    final completer = Completer<AuthTokensModel?>();
    _inFlightRefresh = completer;

    try {
      final refreshToken = await _storage.readRefreshToken();
      if (refreshToken == null || refreshToken.isEmpty) {
        await _clearSession();
        completer.complete(null);
        return completer.future;
      }

      final response = await _dio.post<Map<String, dynamic>>(
        '/auth/refresh',
        data: <String, dynamic>{'refreshToken': refreshToken},
        options: Options(extra: const {'skipAuth': true}),
      );

      final session = AuthTokensModel.fromJson(response.data ?? <String, dynamic>{});
      await _storage.saveAuthTokens(
        accessToken: session.accessToken,
        refreshToken: session.refreshToken,
      );
      await _storage.saveUserProfile(session.user);
      completer.complete(session);
      return completer.future;
    } on DioException catch (error, stackTrace) {
      if (_isSessionExpired(error)) {
        await _clearSession();
        completer.complete(null);
        return completer.future;
      }

      completer.completeError(error, stackTrace);
      return completer.future;
    } catch (error, stackTrace) {
      completer.completeError(error, stackTrace);
      return completer.future;
    } finally {
      _inFlightRefresh = null;
    }
  }

  Future<void> _clearSession() async {
    await _storage.clearAuthSession();
    _onSessionInvalidated();
  }

  bool _isSessionExpired(DioException error) {
    final statusCode = error.response?.statusCode;
    return statusCode == 400 || statusCode == 401;
  }
}
