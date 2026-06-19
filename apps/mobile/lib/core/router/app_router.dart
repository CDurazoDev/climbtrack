import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/auth/presentation/pages/login_page.dart';
import '../../features/auth/presentation/pages/onboarding_page.dart';
import '../../features/auth/presentation/pages/register_page.dart';
import '../../features/auth/presentation/pages/forgot_password_page.dart';
import '../../features/auth/presentation/pages/reset_password_page.dart';
import '../../features/auth/presentation/providers/auth_state.dart';
import '../../features/dashboard/presentation/pages/dashboard_page.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  final routerNotifier = _RouterNotifier();
  ref.listen<AsyncValue<AuthState>>(authStateProvider, (_, __) {
    routerNotifier.refresh();
  });
  ref.onDispose(routerNotifier.dispose);

  return GoRouter(
    initialLocation: '/onboarding',
    refreshListenable: routerNotifier,
    redirect: (context, state) {
      final authState = ref.read(authStateProvider);
      if (authState.isLoading) {
        return null;
      }

      final isAuthenticated = authState.valueOrNull?.isAuthenticated ?? false;
      final location = state.matchedLocation;
      final isAuthRoute = location.startsWith('/auth');
      final isOnboarding = location == '/onboarding';

      if (!isAuthenticated) {
        if (isAuthRoute || isOnboarding) {
          return null;
        }
        return '/auth/login';
      }

      if (isAuthenticated && (isAuthRoute || isOnboarding)) {
        return '/home/dashboard';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/onboarding',
        builder: (context, state) => const OnboardingPage(),
      ),
      GoRoute(
        path: '/auth/login',
        builder: (context, state) => const LoginPage(),
      ),
      GoRoute(
        path: '/auth/register',
        builder: (context, state) => const RegisterPage(),
      ),
      GoRoute(
        path: '/auth/forgot-password',
        builder: (context, state) => const ForgotPasswordPage(),
      ),
      GoRoute(
        path: '/auth/reset-password',
        builder: (context, state) => ResetPasswordPage(
          initialToken: state.uri.queryParameters['token'] ?? '',
        ),
      ),
      GoRoute(
        path: '/home/dashboard',
        builder: (context, state) => const DashboardPage(),
      ),
    ],
  );
});

class _RouterNotifier extends ChangeNotifier {
  void refresh() {
    notifyListeners();
  }
}
