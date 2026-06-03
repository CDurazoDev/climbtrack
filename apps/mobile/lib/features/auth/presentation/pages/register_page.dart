import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_typography.dart';
import '../providers/auth_state.dart';

class RegisterPage extends ConsumerStatefulWidget {
  const RegisterPage({super.key});

  @override
  ConsumerState<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends ConsumerState<RegisterPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;
  String _selectedLevel = 'novato';

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }

    await ref.read(authStateProvider.notifier).register(
          _nameController.text.trim(),
          _emailController.text.trim(),
          _passwordController.text,
          _selectedLevel,
        );
  }

  @override
  Widget build(BuildContext context) {
    final authValue = ref.watch(authStateProvider);
    final authState = authValue.valueOrNull;
    final isSubmitting = authState?.maybeWhen(loading: () => true, orElse: () => false) ?? false;

    ref.listen<AsyncValue<AuthState>>(authStateProvider, (previous, next) {
      next.whenData((state) {
        state.maybeWhen(
          error: (message) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text(message)),
            );
          },
          orElse: () {},
        );
      });
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Crear cuenta'),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: kPaddingH, vertical: 20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Crea tu cuenta',
                  style: AppTypography.headingLg,
                ),
                const SizedBox(height: 8),
                Text(
                  'Elige tu nivel para personalizar tu experiencia',
                  style: AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
                ),
                const SizedBox(height: 32),
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    labelText: 'Nombre completo',
                  ),
                  validator: (value) {
                    if ((value?.trim() ?? '').isEmpty) {
                      return 'Ingresa tu nombre';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(
                    labelText: 'Email',
                  ),
                  validator: (value) {
                    final text = value?.trim() ?? '';
                    if (text.isEmpty) {
                      return 'Ingresa tu email';
                    }
                    if (!text.contains('@') || !text.contains('.')) {
                      return 'Ingresa un email valido';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _passwordController,
                  obscureText: _obscurePassword,
                  decoration: InputDecoration(
                    labelText: 'Contraseña',
                    suffixIcon: IconButton(
                      onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                      icon: Icon(_obscurePassword ? Icons.visibility_off : Icons.visibility),
                    ),
                  ),
                  validator: (value) {
                    final text = value ?? '';
                    if (text.isEmpty) {
                      return 'Ingresa tu contraseña';
                    }
                    if (text.length < 8) {
                      return 'La contraseña debe tener al menos 8 caracteres';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 24),
                Text('Nivel', style: AppTypography.headingSm),
                const SizedBox(height: 12),
                Row(
                  children: [
                    Expanded(
                      child: _LevelCard(
                        label: 'Novato',
                        range: 'V0-V4',
                        color: AppColors.tertiary,
                        selected: _selectedLevel == 'novato',
                        onTap: () => setState(() => _selectedLevel = 'novato'),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _LevelCard(
                        label: 'Intermedio',
                        range: 'V5-V8',
                        color: AppColors.secondary,
                        selected: _selectedLevel == 'intermedio',
                        onTap: () => setState(() => _selectedLevel = 'intermedio'),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: _LevelCard(
                        label: 'Avanzado',
                        range: 'V9+',
                        color: AppColors.primary,
                        selected: _selectedLevel == 'avanzado',
                        onTap: () => setState(() => _selectedLevel = 'avanzado'),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 24),
                ElevatedButton(
                  onPressed: isSubmitting ? null : _submit,
                  child: isSubmitting
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Crear cuenta'),
                ),
                const SizedBox(height: 16),
                TextButton(
                  onPressed: () => context.go('/auth/login'),
                  child: const Text('Ya tengo una cuenta'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _LevelCard extends StatelessWidget {
  const _LevelCard({
    required this.label,
    required this.range,
    required this.color,
    required this.selected,
    required this.onTap,
  });

  final String label;
  final String range;
  final Color color;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: AppColors.surfaceCard,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: selected ? color : Colors.white12,
            width: selected ? 1.8 : 1,
          ),
        ),
        child: Column(
          children: [
            Text(label, textAlign: TextAlign.center, style: AppTypography.bodyMd),
            const SizedBox(height: 4),
            Text(range, textAlign: TextAlign.center, style: AppTypography.bodySm),
          ],
        ),
      ),
    );
  }
}
