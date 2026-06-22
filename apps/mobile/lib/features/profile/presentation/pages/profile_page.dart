import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_decorations.dart';
import '../../../../core/theme/app_typography.dart';
import '../../../auth/presentation/providers/auth_state.dart';

class ProfilePage extends ConsumerWidget {
  const ProfilePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authStateProvider).valueOrNull?.maybeWhen(
          authenticated: (user) => user,
          orElse: () => null,
        );

    return Scaffold(
      backgroundColor: AppColors.surface,
      appBar: AppBar(title: const Text('Perfil')),
      body: Padding(
        padding: const EdgeInsets.all(kPaddingH),
        child: DecoratedBox(
          decoration: AppDecorations.darkCard,
          child: Padding(
            padding: const EdgeInsets.all(kGapLg),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(user?.name ?? 'Usuario', style: AppTypography.headingLg),
                const SizedBox(height: kGapSm),
                Text(user?.email ?? '', style: AppTypography.bodyMd),
                const SizedBox(height: kGapMd),
                Text('Nivel: ${user?.level ?? '-'}',
                    style: AppTypography.bodySm),
                const Spacer(),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: () =>
                        ref.read(authStateProvider.notifier).logout(),
                    child: const Text('Cerrar sesión'),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
