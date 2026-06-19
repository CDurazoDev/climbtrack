import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/error/failure.dart';
import '../../../../core/storage/secure_storage_provider.dart';
import '../../data/models/auth_tokens_model.dart';
import 'auth_providers.dart';
import 'session_providers.dart';

sealed class AuthState {
  const AuthState();

  const factory AuthState.initial() = AuthInitial;
  const factory AuthState.loading() = AuthLoading;
  const factory AuthState.authenticated(UserProfileModel user) = AuthAuthenticated;
  const factory AuthState.unauthenticated() = AuthUnauthenticated;
  const factory AuthState.error(String message) = AuthError;
}

class AuthInitial extends AuthState {
  const AuthInitial();
}

class AuthLoading extends AuthState {
  const AuthLoading();
}

class AuthAuthenticated extends AuthState {
  const AuthAuthenticated(this.user);

  final UserProfileModel user;
}

class AuthUnauthenticated extends AuthState {
  const AuthUnauthenticated();
}

class AuthError extends AuthState {
  const AuthError(this.message);

  final String message;
}

extension AuthStateX on AuthState {
  bool get isAuthenticated => this is AuthAuthenticated;

  T maybeWhen<T>({
    T Function()? initial,
    T Function()? loading,
    T Function(UserProfileModel user)? authenticated,
    T Function()? unauthenticated,
    T Function(String message)? error,
    required T Function() orElse,
  }) {
    final state = this;
    if (state is AuthInitial && initial != null) {
      return initial();
    }
    if (state is AuthLoading && loading != null) {
      return loading();
    }
    if (state is AuthAuthenticated && authenticated != null) {
      return authenticated(state.user);
    }
    if (state is AuthUnauthenticated && unauthenticated != null) {
      return unauthenticated();
    }
    if (state is AuthError && error != null) {
      return error(state.message);
    }
    return orElse();
  }
}

class AuthNotifier extends AsyncNotifier<AuthState> {
  @override
  Future<AuthState> build() async {
    ref.watch(sessionVersionProvider);

    final storage = ref.read(secureStorageProvider);
    final accessToken = await storage.readAccessToken();
    final refreshToken = await storage.readRefreshToken();
    final user = await storage.readUserProfile();

    if (accessToken == null || accessToken.isEmpty || user == null) {
      return const AuthState.unauthenticated();
    }

    if (refreshToken == null || refreshToken.isEmpty) {
      return AuthState.authenticated(user);
    }

    final refreshResult = await ref.read(authRepositoryProvider).refreshSession();
    return refreshResult.fold(
      (failure) {
        if (failure is SessionExpiredFailure) {
          return const AuthState.unauthenticated();
        }

        return AuthState.authenticated(user);
      },
      (session) => AuthState.authenticated(session.user),
    );
  }

  Future<void> refreshSession() async {
    final currentUser = await ref.read(authRepositoryProvider).getCurrentUser();
    final fallbackUser = currentUser.fold((_) => null, (profile) => profile);

    state = const AsyncValue.data(AuthState.loading());
    final result = await ref.read(authRepositoryProvider).refreshSession();
    state = result.fold(
      (failure) {
        if (failure is SessionExpiredFailure) {
          return const AsyncValue.data(AuthState.unauthenticated());
        }

        if (fallbackUser != null) {
          return AsyncValue.data(AuthState.authenticated(fallbackUser));
        }

        return AsyncValue.data(AuthState.error(failure.message));
      },
      (session) => AsyncValue.data(AuthState.authenticated(session.user)),
    );
  }

  Future<void> login(String email, String password) async {
    state = const AsyncValue.data(AuthState.loading());
    final result = await ref
        .read(authRepositoryProvider)
        .login(email: email, password: password);
    state = result.fold(
      (failure) => AsyncValue.data(AuthState.error(failure.message)),
      (session) => AsyncValue.data(AuthState.authenticated(session.user)),
    );
  }

  Future<void> register(String name, String email, String password, String level) async {
    state = const AsyncValue.data(AuthState.loading());
    final result = await ref.read(authRepositoryProvider).register(
          name: name,
          email: email,
          password: password,
          level: level,
        );
    state = result.fold(
      (failure) => AsyncValue.data(AuthState.error(failure.message)),
      (session) => AsyncValue.data(AuthState.authenticated(session.user)),
    );
  }

  Future<void> logout() async {
    await ref.read(authRepositoryProvider).logout();
    state = const AsyncValue.data(AuthState.unauthenticated());
  }
}

final authStateProvider = AsyncNotifierProvider<AuthNotifier, AuthState>(
  AuthNotifier.new,
);
