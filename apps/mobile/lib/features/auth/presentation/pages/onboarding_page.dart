import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_typography.dart';

class OnboardingPage extends StatefulWidget {
  const OnboardingPage({super.key});

  @override
  State<OnboardingPage> createState() => _OnboardingPageState();
}

class _OnboardingPageState extends State<OnboardingPage> {
  final PageController _pageController = PageController();
  int _currentPage = 0;

  final List<_OnboardingSlide> _slides = const [
    _OnboardingSlide(
      title: 'Periodiza como un profesional',
      subtitle: 'Sigue un ciclo anual de Base -> Fuerza -> Potencia -> Performance',
      accentColor: AppColors.secondary,
      icon: Icons.timeline_rounded,
    ),
    _OnboardingSlide(
      title: 'Cada sesión, registrada',
      subtitle: 'Registra métricas específicas de cada tipo de entrenamiento',
      accentColor: AppColors.tertiary,
      icon: Icons.fitness_center_rounded,
    ),
    _OnboardingSlide(
      title: 'Tu progreso, visible',
      subtitle: 'Analiza tu carga, RPE y distribución de energía',
      accentColor: AppColors.primary,
      icon: Icons.bar_chart_rounded,
    ),
  ];

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  void _goNext() {
    if (_currentPage < _slides.length - 1) {
      _pageController.nextPage(
        duration: const Duration(milliseconds: 320),
        curve: Curves.easeOutCubic,
      );
      return;
    }

    context.go('/auth/register');
  }

  @override
  Widget build(BuildContext context) {
    final isLastPage = _currentPage == _slides.length - 1;

    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: kPaddingH),
          child: Column(
            children: [
              Align(
                alignment: Alignment.centerRight,
                child: TextButton(
                  onPressed: () => context.go('/auth/login'),
                  child: const Text('Saltar'),
                ),
              ),
              Expanded(
                child: PageView.builder(
                  controller: _pageController,
                  itemCount: _slides.length,
                  onPageChanged: (value) => setState(() => _currentPage = value),
                  itemBuilder: (context, index) {
                    final slide = _slides[index];
                    return Padding(
                      padding: const EdgeInsets.only(top: 24),
                      child: _SlideContent(slide: slide),
                    );
                  },
                ),
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(
                  _slides.length,
                  (index) => AnimatedContainer(
                    duration: const Duration(milliseconds: 250),
                    margin: const EdgeInsets.symmetric(horizontal: 4),
                    height: 8,
                    width: _currentPage == index ? 22 : 8,
                    decoration: BoxDecoration(
                      color: _currentPage == index ? AppColors.primary : Colors.white24,
                      borderRadius: BorderRadius.circular(999),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: _goNext,
                  child: Text(isLastPage ? 'Empezar' : 'Siguiente'),
                ),
              ),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
    );
  }
}

class _SlideContent extends StatelessWidget {
  const _SlideContent({required this.slide});

  final _OnboardingSlide slide;

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Container(
          width: 220,
          height: 220,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(32),
            gradient: LinearGradient(
              colors: [
                slide.accentColor.withValues(alpha: 0.20),
                slide.accentColor.withValues(alpha: 0.04),
              ],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            border: Border.all(color: slide.accentColor.withValues(alpha: 0.25)),
          ),
          child: Icon(slide.icon, size: 92, color: slide.accentColor),
        ),
        const SizedBox(height: 28),
        Text(
          slide.title,
          textAlign: TextAlign.center,
          style: AppTypography.headingLg,
        ),
        const SizedBox(height: 12),
        Text(
          slide.subtitle,
          textAlign: TextAlign.center,
          style: AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
        ),
      ],
    );
  }
}

class _OnboardingSlide {
  const _OnboardingSlide({
    required this.title,
    required this.subtitle,
    required this.accentColor,
    required this.icon,
  });

  final String title;
  final String subtitle;
  final Color accentColor;
  final IconData icon;
}
