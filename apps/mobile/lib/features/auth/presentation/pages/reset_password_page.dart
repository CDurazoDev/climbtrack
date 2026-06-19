import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_typography.dart';
import '../providers/password_reset_provider.dart';

class ResetPasswordPage extends ConsumerStatefulWidget {
  const ResetPasswordPage({
    super.key,
    this.initialToken = '',
  });

  final String initialToken;

  @override
  ConsumerState<ResetPasswordPage> createState() => _ResetPasswordPageState();
}

class _ResetPasswordPageState extends ConsumerState<ResetPasswordPage> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _tokenController;
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  bool _obscurePassword = true;
  bool _obscureConfirmation = true;

  @override
  void initState() {
    super.initState();
    _tokenController = TextEditingController(text: widget.initialToken);
  }

  @override
  void dispose() {
    _tokenController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }

    await ref.read(passwordResetProvider.notifier).resetPassword(
          token: _tokenController.text.trim(),
          newPassword: _passwordController.text,
          confirmPassword: _confirmPasswordController.text,
        );
  }

  @override
  Widget build(BuildContext context) {
    final resetValue = ref.watch(passwordResetProvider);
    final resetState = resetValue.valueOrNull;
    final isSubmitting = resetState?.maybeWhen(loading: () => true, orElse: () => false) ?? false;

    ref.listen<AsyncValue<PasswordResetState>>(passwordResetProvider, (previous, next) {
      next.whenData((state) {
        state.maybeWhen(
          success: (message) {
            ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
            ref.read(passwordResetProvider.notifier).resetState();
            context.go('/auth/login');
          },
          error: (message) {
            ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
          },
          orElse: () {},
        );
      });
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('Nueva contraseña'),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: kPaddingH, vertical: 20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text(
                  'Restablecer contraseña',
                  style: AppTypography.headingLg,
                ),
                const SizedBox(height: 8),
                const Text(
                  'Pega el token que recibiste por correo y define una nueva contraseña.',
                  style: AppTypography.bodyMd,
                ),
                const SizedBox(height: 32),
                TextFormField(
                  controller: _tokenController,
                  decoration: const InputDecoration(
                    labelText: 'Token',
                  ),
                  validator: (value) {
                    if ((value?.trim() ?? '').isEmpty) {
                      return 'Ingresa el token';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _passwordController,
                  obscureText: _obscurePassword,
                  decoration: InputDecoration(
                    labelText: 'Nueva contraseña',
                    suffixIcon: IconButton(
                      onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
                      icon: Icon(_obscurePassword ? Icons.visibility_off : Icons.visibility),
                    ),
                  ),
                  validator: (value) {
                    final text = value ?? '';
                    if (text.isEmpty) {
                      return 'Ingresa tu nueva contraseña';
                    }
                    if (text.length < 8) {
                      return 'La contraseña debe tener al menos 8 caracteres';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16),
                TextFormField(
                  controller: _confirmPasswordController,
                  obscureText: _obscureConfirmation,
                  decoration: InputDecoration(
                    labelText: 'Confirmar contraseña',
                    suffixIcon: IconButton(
                      onPressed: () => setState(() => _obscureConfirmation = !_obscureConfirmation),
                      icon: Icon(_obscureConfirmation ? Icons.visibility_off : Icons.visibility),
                    ),
                  ),
                  validator: (value) {
                    if ((value ?? '').isEmpty) {
                      return 'Confirma tu nueva contraseña';
                    }
                    if (value != _passwordController.text) {
                      return 'Las contraseñas no coinciden';
                    }
                    return null;
                  },
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
                      : const Text('Actualizar contraseña'),
                ),
                const SizedBox(height: 12),
                TextButton(
                  onPressed: () => context.go('/auth/login'),
                  child: const Text('Volver a iniciar sesión'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
