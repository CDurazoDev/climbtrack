import 'package:dio/dio.dart';

import '../models/auth_tokens_model.dart';

class AuthRemoteDatasource {
  AuthRemoteDatasource(this._dio);

  final Dio _dio;

  Future<AuthTokensModel> register(RegisterRequest request) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/auth/register',
      data: request.toJson(),
      options: Options(extra: const {'skipAuth': true}),
    );
    return AuthTokensModel.fromJson(response.data ?? <String, dynamic>{});
  }

  Future<AuthTokensModel> login(LoginRequest request) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/auth/login',
      data: request.toJson(),
      options: Options(extra: const {'skipAuth': true}),
    );
    return AuthTokensModel.fromJson(response.data ?? <String, dynamic>{});
  }

  Future<AuthTokensModel> refresh(RefreshRequest request) async {
    final response = await _dio.post<Map<String, dynamic>>(
      '/auth/refresh',
      data: request.toJson(),
      options: Options(extra: const {'skipAuth': true}),
    );
    return AuthTokensModel.fromJson(response.data ?? <String, dynamic>{});
  }

  Future<void> logout(LogoutRequest request) async {
    await _dio.post<void>(
      '/auth/logout',
      data: request.toJson(),
      options: Options(extra: const {'skipAuth': true}),
    );
  }

  Future<void> forgotPassword(ForgotPasswordRequest request) async {
    await _dio.post<void>(
      '/auth/forgot-password',
      data: request.toJson(),
      options: Options(extra: const {'skipAuth': true}),
    );
  }

  Future<void> resetPassword(ResetPasswordRequest request) async {
    await _dio.post<void>(
      '/auth/reset-password',
      data: request.toJson(),
      options: Options(extra: const {'skipAuth': true}),
    );
  }
}

class RegisterRequest {
  const RegisterRequest({
    required this.name,
    required this.email,
    required this.password,
    required this.level,
  });

  final String name;
  final String email;
  final String password;
  final String level;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'name': name,
      'email': email,
      'password': password,
      'level': level,
    };
  }
}

class LoginRequest {
  const LoginRequest({
    required this.email,
    required this.password,
  });

  final String email;
  final String password;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'email': email,
      'password': password,
    };
  }
}

class RefreshRequest {
  const RefreshRequest({
    required this.refreshToken,
  });

  final String refreshToken;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'refreshToken': refreshToken,
    };
  }
}

class LogoutRequest {
  const LogoutRequest({
    required this.refreshToken,
  });

  final String refreshToken;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'refreshToken': refreshToken,
    };
  }
}

class ForgotPasswordRequest {
  const ForgotPasswordRequest({
    required this.email,
  });

  final String email;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'email': email,
    };
  }
}

class ResetPasswordRequest {
  const ResetPasswordRequest({
    required this.token,
    required this.newPassword,
    required this.confirmPassword,
  });

  final String token;
  final String newPassword;
  final String confirmPassword;

  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'token': token,
      'newPassword': newPassword,
      'confirmPassword': confirmPassword,
    };
  }
}
