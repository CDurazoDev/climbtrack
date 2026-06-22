import 'package:dio/dio.dart';

import '../../../dashboard/data/models/dashboard_models.dart';
import '../models/session_log_models.dart';

class SessionLogRemoteDatasource {
  SessionLogRemoteDatasource(this._dio);

  final Dio _dio;

  Future<SessionLogDto?> getTodaySession() async {
    try {
      final response =
          await _dio.get<Map<String, dynamic>>('/api/v1/session-logs/today');
      final data = response.data;
      if (data == null || data.isEmpty) {
        return null;
      }

      return SessionLogDto.fromJson(data);
    } on DioException catch (error) {
      if (error.response?.statusCode == 404) {
        return null;
      }

      rethrow;
    }
  }

  Future<List<SessionTypeDto>> getSessionTypes() async {
    final response =
        await _dio.get<List<dynamic>>('/api/v1/catalogs/session-types');
    return (response.data ?? const [])
        .map((item) => SessionTypeDto.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<List<SessionLogDto>> getWeekSessionLogs(int planWeekId) async {
    final response = await _dio.get<List<dynamic>>(
      '/api/v1/session-logs/',
      queryParameters: {'planWeekId': planWeekId},
    );

    return (response.data ?? const [])
        .map((item) => SessionLogDto.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<SessionLogDto> createSessionLog(CreateSessionLogRequest request) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/session-logs/',
      data: request.toJson(),
    );

    return SessionLogDto.fromJson(response.data ?? <String, dynamic>{});
  }

  Future<SessionLogDto> completeSession({
    required int id,
    required CompleteSessionRequest request,
  }) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/api/v1/session-logs/$id/complete',
      data: request.toJson(),
    );

    return SessionLogDto.fromJson(response.data ?? <String, dynamic>{});
  }
}
