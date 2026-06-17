import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_typography.dart';
import '../providers/password_reset_provider.dart';

class ForgotPasswordPage extends ConsumerStatefulWidget {
  const ForgotPasswordPage({super.key});

  @override
  ConsumerState<ForgotPasswordPage> createState() => _ForgotPasswordPageState();
}

class _ForgotPasswordPageState extends ConsumerState<ForgotPasswordPage> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!(_formKey.currentState?.validate() ?? false)) {
      return;
    }

    await ref.read(passwordResetProvider.notifier).sendResetInstructions(
          _emailController.text.trim(),
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
        title: const Text('Recuperar acceso'),
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
                  'Olvidaste tu contrasena',
                  style: AppTypography.headingLg,
                ),
                const SizedBox(height: 8),
                const Text(
                  'Ingresa tu email y te enviaremos un token para restablecerla.',
                  style: AppTypography.bodyMd,
                ),
                const SizedBox(height: 32),
                TextFormField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(
                    labelText: 'Email',
                    hintText: 'tu@email.com',
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
                const SizedBox(height: 20),
                ElevatedButton(
                  onPressed: isSubmitting ? null : _submit,
                  child: isSubmitting
                      ? const SizedBox(
                          height: 20,
                          width: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Enviar instrucciones'),
                ),
                const SizedBox(height: 12),
                TextButton(
                  onPressed: () => context.go('/auth/reset-password'),
                  child: const Text('Ya tengo el token'),
                ),
                TextButton(
                  onPressed: () => context.go('/auth/login'),
                  child: const Text('Volver a iniciar sesion'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
