import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../../../core/error/failure.dart';
import '../../../../core/storage/secure_storage_provider.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/auth_remote_datasource.dart';
import '../models/auth_tokens_model.dart';

class AuthRepositoryImpl implements AuthRepository {
  AuthRepositoryImpl(this._remote, this._storage);

  final AuthRemoteDatasource _remote;
  final FlutterSecureStorage _storage;

  @override
  Future<Either<Failure, AuthTokensModel>> register({
    required String name,
    required String email,
    required String password,
    required String level,
  }) async {
    try {
      final result = await _remote.register(
        RegisterRequest(
          name: name,
          email: email,
          password: password,
          level: level,
        ),
      );
      await _persistSession(result);
      return Right(result);
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  @override
  Future<Either<Failure, AuthTokensModel>> login({
    required String email,
    required String password,
  }) async {
    try {
      final result = await _remote.login(
        LoginRequest(email: email, password: password),
      );
      await _persistSession(result);
      return Right(result);
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> logout() async {
    try {
      final refreshToken = await _storage.readRefreshToken();
      if (refreshToken != null && refreshToken.isNotEmpty) {
        await _remote.logout(LogoutRequest(refreshToken: refreshToken));
      }
      await _storage.clearAuthSession();
      return const Right(null);
    } on DioException catch (error) {
      await _storage.clearAuthSession();
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      await _storage.clearAuthSession();
      return Left(ServerFailure(error.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> forgotPassword({
    required String email,
  }) async {
    try {
      await _remote.forgotPassword(ForgotPasswordRequest(email: email));
      return const Right(null);
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> resetPassword({
    required String token,
    required String newPassword,
    required String confirmPassword,
  }) async {
    try {
      await _remote.resetPassword(
        ResetPasswordRequest(
          token: token,
          newPassword: newPassword,
          confirmPassword: confirmPassword,
        ),
      );
      await _storage.clearAuthSession();
      return const Right(null);
    } on DioException catch (error) {
      return Left(ServerFailure(_extractMessage(error)));
    } catch (error) {
      return Left(ServerFailure(error.toString()));
    }
  }

  @override
  Future<Either<Failure, UserProfileModel>> getCurrentUser() async {
    try {
      final user = await _storage.readUserProfile();
      if (user == null) {
        return const Left(AuthFailure('No authenticated user found.'));
      }

      return Right(user);
    } catch (error) {
      return Left(AuthFailure(error.toString()));
    }
  }

  Future<void> _persistSession(AuthTokensModel session) async {
    await _storage.saveAuthTokens(
      accessToken: session.accessToken,
      refreshToken: session.refreshToken,
    );
    await _storage.saveUserProfile(session.user);
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
