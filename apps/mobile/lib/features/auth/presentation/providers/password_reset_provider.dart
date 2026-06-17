import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'auth_providers.dart';

sealed class PasswordResetState {
  const PasswordResetState();

  const factory PasswordResetState.idle() = PasswordResetIdle;
  const factory PasswordResetState.loading() = PasswordResetLoading;
  const factory PasswordResetState.success(String message) = PasswordResetSuccess;
  const factory PasswordResetState.error(String message) = PasswordResetError;
}

class PasswordResetIdle extends PasswordResetState {
  const PasswordResetIdle();
}

class PasswordResetLoading extends PasswordResetState {
  const PasswordResetLoading();
}

class PasswordResetSuccess extends PasswordResetState {
  const PasswordResetSuccess(this.message);

  final String message;
}

class PasswordResetError extends PasswordResetState {
  const PasswordResetError(this.message);

  final String message;
}

extension PasswordResetStateX on PasswordResetState {
  T maybeWhen<T>({
    T Function()? idle,
    T Function()? loading,
    T Function(String message)? success,
    T Function(String message)? error,
    required T Function() orElse,
  }) {
    final currentState = this;

    if (currentState is PasswordResetIdle && idle != null) {
      return idle();
    }

    if (currentState is PasswordResetLoading && loading != null) {
      return loading();
    }

    if (currentState is PasswordResetSuccess && success != null) {
      return success(currentState.message);
    }

    if (currentState is PasswordResetError && error != null) {
      return error(currentState.message);
    }

    return orElse();
  }
}

class PasswordResetNotifier extends AsyncNotifier<PasswordResetState> {
  @override
  Future<PasswordResetState> build() async {
    return const PasswordResetState.idle();
  }

  Future<void> sendResetInstructions(String email) async {
    state = const AsyncValue.data(PasswordResetState.loading());
    final result = await ref.read(authRepositoryProvider).forgotPassword(email: email);
    state = result.fold(
      (failure) => AsyncValue.data(PasswordResetState.error(failure.message)),
      (_) => const AsyncValue.data(
        PasswordResetState.success(
          'Si la cuenta existe, recibiras instrucciones por correo.',
        ),
      ),
    );
  }

  Future<void> resetPassword({
    required String token,
    required String newPassword,
    required String confirmPassword,
  }) async {
    state = const AsyncValue.data(PasswordResetState.loading());
    final result = await ref.read(authRepositoryProvider).resetPassword(
          token: token,
          newPassword: newPassword,
          confirmPassword: confirmPassword,
        );
    state = result.fold(
      (failure) => AsyncValue.data(PasswordResetState.error(failure.message)),
      (_) => const AsyncValue.data(
        PasswordResetState.success(
          'Contrasena actualizada. Inicia sesion nuevamente.',
        ),
      ),
    );
  }

  void resetState() {
    state = const AsyncValue.data(PasswordResetState.idle());
  }
}

final passwordResetProvider =
    AsyncNotifierProvider<PasswordResetNotifier, PasswordResetState>(
      PasswordResetNotifier.new,
    );
