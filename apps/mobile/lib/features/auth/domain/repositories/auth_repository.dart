import 'package:dartz/dartz.dart';

import '../../../../core/error/failure.dart';
import '../../data/models/auth_tokens_model.dart';

abstract class AuthRepository {
  Future<Either<Failure, AuthTokensModel>> register({
    required String name,
    required String email,
    required String password,
    required String level,
  });

  Future<Either<Failure, AuthTokensModel>> login({
    required String email,
    required String password,
  });

  Future<Either<Failure, void>> logout();

  Future<Either<Failure, UserProfileModel>> getCurrentUser();
}
