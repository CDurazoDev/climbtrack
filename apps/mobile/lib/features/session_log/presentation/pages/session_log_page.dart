import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_decorations.dart';
import '../../../../core/theme/app_typography.dart';
import '../../../../core/theme/color_extensions.dart';
import '../../../../shared/widgets/metric_input_field.dart';
import '../../data/models/session_log_models.dart';
import '../providers/session_log_provider.dart';

class SessionLogPage extends ConsumerWidget {
  const SessionLogPage({
    super.key,
    required this.weekId,
    required this.dayOfWeek,
  });

  final int weekId;
  final int dayOfWeek;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final sessionAsync = ref.watch(
      daySessionProvider((weekId: weekId, day: dayOfWeek)),
    );

    return sessionAsync.when(
      loading: () => const Scaffold(
        backgroundColor: Colors.transparent,
        body: Center(child: CircularProgressIndicator()),
      ),
      error: (error, _) => Scaffold(
        backgroundColor: Colors.transparent,
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(kPaddingH),
            child: Text(
              error.toString().replaceFirst('Exception: ', ''),
              style: AppTypography.bodyMd,
              textAlign: TextAlign.center,
            ),
          ),
        ),
      ),
      data: (session) {
        final state = ref.watch(sessionLogProvider(session));
        final notifier = ref.watch(sessionLogProvider(session).notifier);
        final sessionColor =
            session.sessionType.colorHex.toAppColor(fallback: AppColors.primary);

        ref.listen<SessionLogState>(sessionLogProvider(session), (previous, next) {
          if (next.errorMessage != null && next.errorMessage != previous?.errorMessage) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text(next.errorMessage!)),
            );
          }

          if (previous?.isSaved != true && next.isSaved) {
            _handleSaveSuccess(context);
          }
        });

        return Scaffold(
          backgroundColor: Colors.transparent,
          body: Material(
            color: Colors.black.withValues(alpha: 0.72),
            child: SafeArea(
              bottom: false,
              child: Align(
                alignment: Alignment.bottomCenter,
                child: Container(
                  height: MediaQuery.of(context).size.height * 0.92,
                  decoration: const BoxDecoration(
                    color: AppColors.surfaceVariant,
                    borderRadius: BorderRadius.vertical(
                      top: Radius.circular(24),
                    ),
                  ),
                  child: Column(
                    children: [
                      _SessionLogHeader(
                        session: session,
                        state: state,
                        onClose: () => context.pop(),
                      ),
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: kPaddingH),
                        child: _StepProgressBar(
                          step: state.step,
                          sessionColor: sessionColor,
                        ),
                      ),
                      Expanded(
                        child: switch (state.step) {
                          SessionLogStep.checklist => _Step1Checklist(
                              session: session,
                              state: state,
                              notifier: notifier,
                              sessionColor: sessionColor,
                            ),
                          SessionLogStep.metrics => _Step2Metrics(
                              session: session,
                              state: state,
                              notifier: notifier,
                              sessionColor: sessionColor,
                            ),
                          SessionLogStep.wrapup => _Step3Wrapup(
                              state: state,
                              notifier: notifier,
                              session: session,
                            ),
                        },
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  void _handleSaveSuccess(BuildContext context) async {
    await _showCelebrationDialog(context);
    if (context.mounted) {
      context.pop();
    }
  }
}

class _SessionLogHeader extends StatelessWidget {
  const _SessionLogHeader({
    required this.session,
    required this.state,
    required this.onClose,
  });

  final DaySessionDetails session;
  final SessionLogState state;
  final VoidCallback onClose;

  @override
  Widget build(BuildContext context) {
    final sessionColor =
        session.sessionType.colorHex.toAppColor(fallback: AppColors.primary);

    return Padding(
      padding: const EdgeInsets.fromLTRB(kPaddingH, kGapLg, kPaddingH, kGapMd),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              InkWell(
                onTap: onClose,
                borderRadius: BorderRadius.circular(999),
                child: Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: AppColors.surfaceCard,
                    shape: BoxShape.circle,
                    border: Border.all(color: Colors.white.withValues(alpha: 0.08)),
                  ),
                  child: const Icon(
                    Icons.close_rounded,
                    color: AppColors.textPrimary,
                  ),
                ),
              ),
              const SizedBox(width: kGapMd),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Registro de sesión',
                      style: AppTypography.bodySm,
                    ),
                    const SizedBox(height: 4),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                      decoration: BoxDecoration(
                        color: sessionColor.withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: Text(
                        session.sessionType.name,
                        style: AppTypography.labelCaps.copyWith(
                          color: sessionColor,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: kGapMd),
              _LoadIndicator(
                level: session.sessionType.loadLevel,
                color: sessionColor,
              ),
            ],
          ),
          const SizedBox(height: kGapMd),
          Text(
            '${state.completedChecklistItems}/${state.totalChecklistItems} ítems completados',
            style: AppTypography.bodySm.copyWith(color: AppColors.textSecondary),
          ),
        ],
      ),
    );
  }
}

class _StepProgressBar extends StatelessWidget {
  const _StepProgressBar({
    required this.step,
    required this.sessionColor,
  });

  final SessionLogStep step;
  final Color sessionColor;

  @override
  Widget build(BuildContext context) {
    final activeIndex = switch (step) {
      SessionLogStep.checklist => 0,
      SessionLogStep.metrics => 1,
      SessionLogStep.wrapup => 2,
    };

    return Row(
      children: List.generate(3, (index) {
        final isActive = index <= activeIndex;
        return Expanded(
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 220),
            margin: EdgeInsets.only(right: index == 2 ? 0 : 8),
            height: 6,
            decoration: BoxDecoration(
              color: isActive ? sessionColor : AppColors.surfaceCard,
              borderRadius: BorderRadius.circular(999),
            ),
          ),
        );
      }),
    );
  }
}

class _Step1Checklist extends StatelessWidget {
  const _Step1Checklist({
    required this.session,
    required this.state,
    required this.notifier,
    required this.sessionColor,
  });

  final DaySessionDetails session;
  final SessionLogState state;
  final SessionLogNotifier notifier;
  final Color sessionColor;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(kPaddingH, kGapLg, kPaddingH, kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Paso 1 · Checklist', style: AppTypography.headingSm),
          const SizedBox(height: kGapSm),
          const Text(
            'Marca al menos un elemento por bloque para continuar.',
            style: AppTypography.bodySm,
          ),
          const SizedBox(height: kGapLg),
          Expanded(
            child: ListView.separated(
              itemCount: session.sessionType.blocks.length,
              separatorBuilder: (_, __) => const SizedBox(height: kGapMd),
              itemBuilder: (context, index) {
                final block = session.sessionType.blocks[index];
                final blockState = state.checklist[block.id.toString()] ?? const {};
                return _ChecklistBlockCard(
                  block: block,
                  blockState: blockState,
                  sessionColor: sessionColor,
                  onToggle: (item) => notifier.toggleCheck(block.id.toString(), item),
                );
              },
            ),
          ),
          const SizedBox(height: kGapLg),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: state.isChecklistComplete ? notifier.nextStep : null,
              child: const Text('Siguiente'),
            ),
          ),
        ],
      ),
    );
  }
}

class _ChecklistBlockCard extends StatefulWidget {
  const _ChecklistBlockCard({
    required this.block,
    required this.blockState,
    required this.sessionColor,
    required this.onToggle,
  });

  final SessionBlockDto block;
  final Map<String, bool> blockState;
  final Color sessionColor;
  final ValueChanged<String> onToggle;

  @override
  State<_ChecklistBlockCard> createState() => _ChecklistBlockCardState();
}

class _ChecklistBlockCardState extends State<_ChecklistBlockCard> {
  bool _expanded = true;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: AppDecorations.darkCard,
      child: Column(
        children: [
          InkWell(
            onTap: () => setState(() => _expanded = !_expanded),
            borderRadius: BorderRadius.circular(kRadiusLg),
            child: Padding(
              padding: const EdgeInsets.all(kGapMd),
              child: Row(
                children: [
                  Expanded(
                    child: Text(
                      _blockName(widget.block.name),
                      style: AppTypography.headingSm.copyWith(fontSize: 18),
                    ),
                  ),
                  AnimatedRotation(
                    turns: _expanded ? 0.5 : 0,
                    duration: const Duration(milliseconds: 220),
                    child: const Icon(
                      Icons.keyboard_arrow_down_rounded,
                      color: AppColors.textPrimary,
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (_expanded)
            Padding(
              padding: const EdgeInsets.fromLTRB(kGapMd, 0, kGapMd, kGapMd),
              child: Column(
                children: widget.block.items.map((item) {
                  final checked = widget.blockState[item] ?? false;
                  return Padding(
                    padding: const EdgeInsets.only(top: kGapSm),
                    child: InkWell(
                      onTap: () => widget.onToggle(item),
                      borderRadius: BorderRadius.circular(kRadiusMd),
                      child: Container(
                        padding: const EdgeInsets.all(kGapMd),
                        decoration: BoxDecoration(
                          color: checked
                              ? widget.sessionColor.withValues(alpha: 0.15)
                              : AppColors.surfaceCard,
                          borderRadius: BorderRadius.circular(kRadiusMd),
                          border: Border.all(
                            color: checked
                                ? widget.sessionColor
                                : Colors.white.withValues(alpha: 0.15),
                          ),
                        ),
                        child: Row(
                          children: [
                            Container(
                              width: 22,
                              height: 22,
                              decoration: BoxDecoration(
                                shape: BoxShape.circle,
                                color: checked
                                    ? widget.sessionColor.withValues(alpha: 0.15)
                                    : Colors.transparent,
                                border: Border.all(
                                  color: checked
                                      ? widget.sessionColor
                                      : Colors.white.withValues(alpha: 0.18),
                                ),
                              ),
                              child: checked
                                  ? Icon(
                                      Icons.check_rounded,
                                      size: 14,
                                      color: widget.sessionColor,
                                    )
                                  : null,
                            ),
                            const SizedBox(width: kGapMd),
                            Expanded(
                              child: Text(
                                item,
                                style: AppTypography.bodyMd,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                }).toList(),
              ),
            ),
        ],
      ),
    );
  }

  String _blockName(String rawName) {
    return switch (rawName) {
      'warmup' => 'Calentamiento',
      'main' => 'Trabajo principal',
      'cooldown' => 'Vuelta a la calma',
      _ => rawName,
    };
  }
}

class _Step2Metrics extends StatelessWidget {
  const _Step2Metrics({
    required this.session,
    required this.state,
    required this.notifier,
    required this.sessionColor,
  });

  final DaySessionDetails session;
  final SessionLogState state;
  final SessionLogNotifier notifier;
  final Color sessionColor;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(kPaddingH, kGapLg, kPaddingH, kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Paso 2 · Métricas', style: AppTypography.headingSm),
          const SizedBox(height: kGapSm),
          const Text(
            'Completa los datos más relevantes de la sesión.',
            style: AppTypography.bodySm,
          ),
          const SizedBox(height: kGapLg),
          Expanded(
            child: ListView(
              children: [
                ...switch (session.sessionType.metricType) {
                  'arc' => _buildArcMetrics(),
                  'hangboard' => _buildHangboardMetrics(),
                  'limitBoulder' => _buildLimitBoulderMetrics(),
                  'campus' => _buildCampusMetrics(),
                  'outdoor' => _buildOutdoorMetrics(),
                  'rest' => _buildRestMetrics(),
                  _ => _buildFallbackMetrics(),
                },
              ],
            ),
          ),
          const SizedBox(height: kGapLg),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: notifier.prevStep,
                  child: const Text('Atrás'),
                ),
              ),
              const SizedBox(width: kGapMd),
              Expanded(
                child: ElevatedButton(
                  onPressed: notifier.nextStep,
                  child: const Text('Siguiente'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  List<Widget> _buildArcMetrics() {
    final metrics = state.arcMetrics;
    return [
      MetricInputField(
        label: 'Series',
        value: metrics.series,
        min: 1,
        max: 8,
        onChanged: (value) => notifier.updateArc(
          metrics.copyWith(series: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Duración por serie',
        value: metrics.durationMin,
        min: 1,
        max: 90,
        unit: 'min',
        onChanged: (value) => notifier.updateArc(
          metrics.copyWith(durationMin: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      _PumpLevelSelector(
        value: metrics.pumpLevel,
        sessionColor: sessionColor,
        onChanged: (value) => notifier.updateArc(
          metrics.copyWith(pumpLevel: value),
        ),
      ),
    ];
  }

  List<Widget> _buildHangboardMetrics() {
    final metrics = state.hangboardMetrics;
    return [
      _DropdownField(
        label: 'Tipo de agarre',
        value: metrics.gripType,
        items: const ['Punta 4 dedos', 'Semis', 'Crimps', 'Pinzas', 'Slopers'],
        onChanged: (value) => notifier.updateHangboard(
          metrics.copyWith(gripType: value),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Peso añadido',
        value: metrics.addedWeightKg,
        unit: 'kg',
        step: 2.5,
        allowNegative: true,
        min: -20,
        max: 80,
        onChanged: (value) => notifier.updateHangboard(
          metrics.copyWith(addedWeightKg: value.toDouble()),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Series',
        value: metrics.sets,
        min: 1,
        max: 12,
        onChanged: (value) => notifier.updateHangboard(
          metrics.copyWith(sets: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Repeticiones',
        value: metrics.reps,
        min: 1,
        max: 12,
        onChanged: (value) => notifier.updateHangboard(
          metrics.copyWith(reps: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Tiempo de cuelgue',
        value: metrics.hangDurationS,
        unit: 's',
        min: 3,
        max: 15,
        onChanged: (value) => notifier.updateHangboard(
          metrics.copyWith(hangDurationS: value.round()),
        ),
      ),
    ];
  }

  List<Widget> _buildLimitBoulderMetrics() {
    final metrics = state.limitBoulderMetrics;
    return [
      _TextFieldCard(
        label: 'Grado objetivo',
        initialValue: metrics.targetGrade,
        onChanged: (value) => notifier.updateLimitBoulder(
          metrics.copyWith(targetGrade: value),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Problemas trabajados',
        value: metrics.problemsWorked,
        min: 1,
        max: 10,
        onChanged: (value) => notifier.updateLimitBoulder(
          metrics.copyWith(problemsWorked: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      _SliderCard(
        label: 'Progreso',
        value: metrics.progressPct.toDouble(),
        min: 0,
        max: 100,
        divisions: 10,
        activeColor: sessionColor,
        text: '${metrics.progressPct}%',
        onChanged: (value) => notifier.updateLimitBoulder(
          metrics.copyWith(progressPct: value.round()),
        ),
      ),
    ];
  }

  List<Widget> _buildCampusMetrics() {
    final metrics = state.campusMetrics;
    return [
      _DropdownField(
        label: 'Ejercicio',
        value: metrics.exercise,
        items: const ['1-3-5', '1-4-7', '1-3-6', 'Máximo'],
        onChanged: (value) => notifier.updateCampus(
          metrics.copyWith(exercise: value),
        ),
      ),
      const SizedBox(height: kGapMd),
      _TextFieldCard(
        label: 'Peldaños',
        initialValue: metrics.rungs,
        onChanged: (value) => notifier.updateCampus(
          metrics.copyWith(rungs: value),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Series',
        value: metrics.sets,
        min: 1,
        max: 12,
        onChanged: (value) => notifier.updateCampus(
          metrics.copyWith(sets: value.round()),
        ),
      ),
    ];
  }

  List<Widget> _buildOutdoorMetrics() {
    final metrics = state.outdoorMetrics;
    return [
      _TextFieldCard(
        label: 'Sector',
        initialValue: metrics.sector,
        onChanged: (value) => notifier.updateOutdoor(
          metrics.copyWith(sector: value),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Redpoints',
        value: metrics.redpoints,
        min: 0,
        max: 20,
        onChanged: (value) => notifier.updateOutdoor(
          metrics.copyWith(redpoints: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      MetricInputField(
        label: 'Intentos en proyecto',
        value: metrics.projectAttempts,
        min: 0,
        max: 50,
        onChanged: (value) => notifier.updateOutdoor(
          metrics.copyWith(projectAttempts: value.round()),
        ),
      ),
      const SizedBox(height: kGapMd),
      _DropdownField(
        label: 'Condiciones',
        value: metrics.conditions,
        items: const ['Buenas', 'Húmedo', 'Viento', 'Calor'],
        onChanged: (value) => notifier.updateOutdoor(
          metrics.copyWith(conditions: value),
        ),
      ),
    ];
  }

  List<Widget> _buildRestMetrics() {
    return const [
      _InfoCard(
        text: 'Hoy toca recuperar energía. No necesitas métricas extra para una jornada de descanso.',
      ),
    ];
  }

  List<Widget> _buildFallbackMetrics() {
    return const [
      _InfoCard(
        text: 'Esta sesión no requiere métricas específicas adicionales.',
      ),
    ];
  }
}

class _Step3Wrapup extends StatelessWidget {
  const _Step3Wrapup({
    required this.state,
    required this.notifier,
    required this.session,
  });

  final SessionLogState state;
  final SessionLogNotifier notifier;
  final DaySessionDetails session;

  @override
  Widget build(BuildContext context) {
    final rpeColor = AppColors.rpeColor(state.rpe);

    return Padding(
      padding: const EdgeInsets.fromLTRB(kPaddingH, kGapLg, kPaddingH, kGapLg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Paso 3 · Cierre', style: AppTypography.headingSm),
          const SizedBox(height: kGapSm),
          const Text(
            'Guarda la duración, el esfuerzo y cualquier nota importante.',
            style: AppTypography.bodySm,
          ),
          const SizedBox(height: kGapLg),
          Expanded(
            child: ListView(
              children: [
                MetricInputField(
                  label: 'Duración total',
                  value: state.durationMin,
                  min: 5,
                  max: 300,
                  unit: 'min',
                  onChanged: (value) => notifier.updateDuration(value.round()),
                ),
                const SizedBox(height: kGapMd),
                _SliderCard(
                  label: 'RPE · ${_rpeLabel(state.rpe)}',
                  value: state.rpe.toDouble(),
                  min: 1,
                  max: 10,
                  divisions: 9,
                  activeColor: rpeColor,
                  text: '${state.rpe}/10',
                  onChanged: (value) => notifier.updateRpe(value.round()),
                ),
                const SizedBox(height: kGapMd),
                _NotesCard(
                  initialValue: state.notes,
                  onChanged: notifier.updateNotes,
                ),
                if (state.errorMessage != null) ...[
                  const SizedBox(height: kGapMd),
                  Text(
                    state.errorMessage!,
                    style: AppTypography.bodySm.copyWith(color: AppColors.error),
                  ),
                ],
              ],
            ),
          ),
          const SizedBox(height: kGapLg),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: state.isSaving ? null : notifier.prevStep,
                  child: const Text('Atrás'),
                ),
              ),
              const SizedBox(width: kGapMd),
              Expanded(
                child: ElevatedButton(
                  onPressed: state.isSaving
                      ? null
                      : () => notifier.save(session.sessionLog.id),
                  child: state.isSaving
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Guardar sesión'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _PumpLevelSelector extends StatelessWidget {
  const _PumpLevelSelector({
    required this.value,
    required this.sessionColor,
    required this.onChanged,
  });

  final int value;
  final Color sessionColor;
  final ValueChanged<int> onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: AppDecorations.darkCard,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Nivel de pump', style: AppTypography.bodySm),
          const SizedBox(height: kGapMd),
          Row(
            children: List.generate(5, (index) {
              final current = index + 1;
              final selected = current <= value;
              return Expanded(
                child: Padding(
                  padding: EdgeInsets.only(right: index == 4 ? 0 : 8),
                  child: InkWell(
                    onTap: () => onChanged(current),
                    borderRadius: BorderRadius.circular(999),
                    child: Container(
                      height: 42,
                      decoration: BoxDecoration(
                        color: selected
                            ? sessionColor.withValues(alpha: 0.15)
                            : AppColors.surfaceCard,
                        borderRadius: BorderRadius.circular(999),
                        border: Border.all(
                          color: selected
                              ? sessionColor
                              : Colors.white.withValues(alpha: 0.08),
                        ),
                      ),
                      alignment: Alignment.center,
                      child: Text(
                        '$current',
                        style: AppTypography.metricSm.copyWith(
                          color: selected ? sessionColor : AppColors.textSecondary,
                        ),
                      ),
                    ),
                  ),
                ),
              );
            }),
          ),
        ],
      ),
    );
  }
}

class _DropdownField extends StatelessWidget {
  const _DropdownField({
    required this.label,
    required this.value,
    required this.items,
    required this.onChanged,
  });

  final String label;
  final String value;
  final List<String> items;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: AppDecorations.darkCard,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: AppTypography.bodySm),
          const SizedBox(height: kGapSm),
          DropdownButtonFormField<String>(
            initialValue: items.contains(value) ? value : items.first,
            dropdownColor: AppColors.surfaceCard,
            decoration: InputDecoration(
              filled: true,
              fillColor: AppColors.surfaceCard,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(kRadiusMd),
                borderSide: BorderSide.none,
              ),
            ),
            items: items
                .map(
                  (item) => DropdownMenuItem<String>(
                    value: item,
                    child: Text(item, style: AppTypography.bodyMd),
                  ),
                )
                .toList(),
            onChanged: (nextValue) {
              if (nextValue != null) {
                onChanged(nextValue);
              }
            },
          ),
        ],
      ),
    );
  }
}

class _TextFieldCard extends StatelessWidget {
  const _TextFieldCard({
    required this.label,
    required this.initialValue,
    required this.onChanged,
  });

  final String label;
  final String initialValue;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: AppDecorations.darkCard,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: AppTypography.bodySm),
          const SizedBox(height: kGapSm),
          TextFormField(
            key: ValueKey('$label-$initialValue'),
            initialValue: initialValue,
            onChanged: onChanged,
            style: AppTypography.bodyMd,
            decoration: InputDecoration(
              filled: true,
              fillColor: AppColors.surfaceCard,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(kRadiusMd),
                borderSide: BorderSide.none,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _NotesCard extends StatelessWidget {
  const _NotesCard({
    required this.initialValue,
    required this.onChanged,
  });

  final String initialValue;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: AppDecorations.darkCard,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Notas', style: AppTypography.bodySm),
          const SizedBox(height: kGapSm),
          TextFormField(
            key: ValueKey('notes-$initialValue'),
            initialValue: initialValue,
            onChanged: onChanged,
            maxLines: 3,
            style: AppTypography.bodyMd,
            decoration: InputDecoration(
              hintText: '¿Algo que destacar de esta sesión?',
              hintStyle: AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
              filled: true,
              fillColor: AppColors.surfaceCard,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(kRadiusMd),
                borderSide: BorderSide.none,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _SliderCard extends StatelessWidget {
  const _SliderCard({
    required this.label,
    required this.value,
    required this.min,
    required this.max,
    required this.divisions,
    required this.activeColor,
    required this.text,
    required this.onChanged,
  });

  final String label;
  final double value;
  final double min;
  final double max;
  final int divisions;
  final Color activeColor;
  final String text;
  final ValueChanged<double> onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapMd),
      decoration: AppDecorations.darkCard,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label, style: AppTypography.bodySm),
          const SizedBox(height: kGapSm),
          Row(
            children: [
              Expanded(
                child: SliderTheme(
                  data: SliderTheme.of(context).copyWith(
                    activeTrackColor: activeColor,
                    thumbColor: activeColor,
                    inactiveTrackColor: AppColors.surfaceCard,
                    overlayColor: activeColor.withValues(alpha: 0.18),
                  ),
                  child: Slider(
                    value: value,
                    min: min,
                    max: max,
                    divisions: divisions,
                    label: text,
                    onChanged: onChanged,
                  ),
                ),
              ),
              const SizedBox(width: kGapMd),
              Text(
                text,
                style: AppTypography.metricSm.copyWith(color: activeColor),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _InfoCard extends StatelessWidget {
  const _InfoCard({
    required this.text,
  });

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(kGapLg),
      decoration: AppDecorations.darkCard,
      child: Text(
        text,
        style: AppTypography.bodyMd.copyWith(color: AppColors.textSecondary),
      ),
    );
  }
}

class _LoadIndicator extends StatelessWidget {
  const _LoadIndicator({
    required this.level,
    required this.color,
  });

  final int level;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: List.generate(4, (index) {
        final isActive = index < level;
        return Container(
          width: 8,
          height: 8,
          margin: EdgeInsets.only(left: index == 0 ? 0 : 4),
          decoration: BoxDecoration(
            color: isActive ? color : Colors.white.withValues(alpha: 0.12),
            borderRadius: BorderRadius.circular(999),
          ),
        );
      }),
    );
  }
}

Future<void> _showCelebrationDialog(BuildContext context) async {
  final navigator = Navigator.of(context, rootNavigator: true);
  Future<void>.delayed(const Duration(milliseconds: 700), () {
    if (navigator.canPop()) {
      navigator.pop();
    }
  });

  await showGeneralDialog<void>(
    context: context,
    barrierDismissible: false,
    barrierColor: Colors.transparent,
    transitionDuration: const Duration(milliseconds: 180),
    pageBuilder: (_, __, ___) => const Material(
      color: Colors.transparent,
      child: Center(child: _CelebrationBurst()),
    ),
  );
}

class _CelebrationBurst extends StatelessWidget {
  const _CelebrationBurst();

  static const _colors = [
    AppColors.primary,
    AppColors.secondary,
    AppColors.tertiary,
    AppColors.textPrimary,
  ];

  static const _angles = [-1.2, -0.7, -0.2, 0.2, 0.7, 1.2];

  @override
  Widget build(BuildContext context) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: const Duration(milliseconds: 650),
      builder: (context, value, _) {
        return Stack(
          alignment: Alignment.center,
          children: [
            ...List.generate(_angles.length, (index) {
              final angle = _angles[index];
              final distance = 110 * Curves.easeOut.transform(value);
              final dx = math.cos(angle) * distance;
              final dy = math.sin(angle) * distance;
              return Transform.translate(
                offset: Offset(dx, dy),
                child: Transform.rotate(
                  angle: angle + value,
                  child: Container(
                    width: 14,
                    height: 14,
                    decoration: BoxDecoration(
                      color: _colors[index % _colors.length],
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                ),
              );
            }),
            Transform.scale(
              scale: 0.8 + (value * 0.3),
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 18),
                decoration: BoxDecoration(
                  color: AppColors.surfaceVariant,
                  borderRadius: BorderRadius.circular(kRadiusLg),
                  border: Border.all(color: Colors.white.withValues(alpha: 0.08)),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.celebration_rounded,
                      color: AppColors.primary.withValues(alpha: 0.95),
                    ),
                    const SizedBox(width: kGapSm),
                    Text(
                      'Sesión guardada',
                      style: AppTypography.headingSm.copyWith(fontSize: 18),
                    ),
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}

String _rpeLabel(int rpe) {
  if (rpe <= 2) {
    return 'Muy fácil';
  }
  if (rpe <= 4) {
    return 'Fácil';
  }
  if (rpe <= 6) {
    return 'Moderado';
  }
  if (rpe <= 8) {
    return 'Difícil';
  }
  return 'Máximo';
}
