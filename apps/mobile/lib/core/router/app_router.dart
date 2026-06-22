import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/auth/presentation/pages/login_page.dart';
import '../../features/auth/presentation/pages/onboarding_page.dart';
import '../../features/auth/presentation/pages/register_page.dart';
import '../../features/auth/presentation/pages/forgot_password_page.dart';
import '../../features/auth/presentation/pages/reset_password_page.dart';
import '../../features/auth/presentation/providers/auth_state.dart';
import '../../features/dashboard/presentation/pages/dashboard_page.dart';
import '../../features/dashboard/presentation/pages/home_shell_page.dart';
import '../../features/plan/presentation/pages/plan_page.dart';
import '../../features/profile/presentation/pages/profile_page.dart';
import '../../features/session_log/presentation/pages/session_log_page.dart';

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
      ShellRoute(
        builder: (context, state, child) => HomeShellPage(
          location: state.uri.path,
          child: child,
        ),
        routes: [
          GoRoute(
            path: '/home/dashboard',
            builder: (context, state) => const DashboardPage(),
          ),
          GoRoute(
            path: '/home/plan',
            builder: (context, state) => const PlanPage(),
          ),
        ],
      ),
      GoRoute(
        path: '/profile',
        builder: (context, state) => const ProfilePage(),
      ),
      GoRoute(
        path: '/session-log/:weekId/:dayOfWeek',
        pageBuilder: (context, state) => CustomTransitionPage<void>(
          key: state.pageKey,
          opaque: false,
          barrierDismissible: false,
          barrierColor: Colors.transparent,
          child: SessionLogPage(
            weekId: int.tryParse(state.pathParameters['weekId'] ?? '') ?? 0,
            dayOfWeek: int.tryParse(state.pathParameters['dayOfWeek'] ?? '') ?? 0,
          ),
          transitionsBuilder: (context, animation, secondaryAnimation, child) {
            final offsetAnimation = Tween<Offset>(
              begin: const Offset(0, 1),
              end: Offset.zero,
            ).animate(
              CurvedAnimation(parent: animation, curve: Curves.easeOutCubic),
            );

            return SlideTransition(
              position: offsetAnimation,
              child: FadeTransition(
                opacity: animation,
                child: child,
              ),
            );
          },
        ),
      ),
    ],
  );
});

class _RouterNotifier extends ChangeNotifier {
  void refresh() {
    notifyListeners();
  }
}
