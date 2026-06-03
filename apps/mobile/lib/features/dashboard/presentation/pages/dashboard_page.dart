import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../auth/data/models/auth_tokens_model.dart';
import '../../../auth/presentation/providers/auth_state.dart';

class DashboardPage extends ConsumerWidget {
  const DashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final authState = ref.watch(authStateProvider).valueOrNull;
    final user = authState?.maybeWhen<UserProfileModel?>(
      authenticated: (user) => user,
      orElse: () => null,
    );

    return Scaffold(
      appBar: AppBar(
        title: const Text('ClimbTrack'),
        actions: [
          TextButton(
            onPressed: () => ref.read(authStateProvider.notifier).logout(),
            child: const Text('Salir'),
          ),
        ],
      ),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.terrain_rounded, size: 72, color: AppColors.primary),
              const SizedBox(height: 16),
              Text(
                'Sesión iniciada',
                style: Theme.of(context).textTheme.headlineMedium,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 8),
              Text(
                user == null ? 'Bienvenido' : 'Bienvenido, ${user.name}',
                style: Theme.of(context).textTheme.bodyMedium,
                textAlign: TextAlign.center,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
