import 'package:equatable/equatable.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/network/dio_provider.dart';
import '../../../dashboard/presentation/providers/dashboard_providers.dart';
import '../../../plan/data/datasources/plan_remote_datasource.dart';
import '../../data/datasources/session_log_remote_datasource.dart';
import '../../data/models/session_log_models.dart';
import '../../data/repositories/session_log_repository_impl.dart';
import '../../domain/entities/session_metrics.dart';
import '../../domain/repositories/session_log_repository.dart';

final sessionLogRemoteDatasourceProvider =
    Provider<SessionLogRemoteDatasource>((ref) {
  return SessionLogRemoteDatasource(ref.read(dioProvider));
});

final sessionLogRepositoryProvider = Provider<SessionLogRepository>((ref) {
  return SessionLogRepositoryImpl(
    ref.read(sessionLogRemoteDatasourceProvider),
    PlanRemoteDatasource(ref.read(dioProvider)),
  );
});

final daySessionProvider = FutureProvider.autoDispose
    .family<DaySessionDetails, ({int weekId, int day})>((ref, args) async {
  final result = await ref.read(sessionLogRepositoryProvider).getDaySession(
        weekId: args.weekId,
        dayOfWeek: args.day,
      );

  return result.fold(
    (failure) => throw Exception(failure.message),
    (session) => session,
  );
});

enum SessionLogStep { checklist, metrics, wrapup }

class SessionLogState extends Equatable {
  const SessionLogState({
    required this.step,
    required this.checklist,
    required this.arcMetrics,
    required this.hangboardMetrics,
    required this.limitBoulderMetrics,
    required this.campusMetrics,
    required this.outdoorMetrics,
    this.rpe = 7,
    this.durationMin = 75,
    this.notes = '',
    this.isSaving = false,
    this.isSaved = false,
    this.errorMessage,
  });

  factory SessionLogState.initial(SessionTypeDto session) {
    final checklist = <String, Map<String, bool>>{};
    for (final block in session.blocks) {
      checklist[block.id.toString()] = <String, bool>{
        for (final item in block.items) item: false,
      };
    }

    return SessionLogState(
      step: SessionLogStep.checklist,
      checklist: checklist,
      arcMetrics: const ArcMetrics(),
      hangboardMetrics: const HangboardMetrics(),
      limitBoulderMetrics: const LimitBoulderMetrics(),
      campusMetrics: const CampusMetrics(),
      outdoorMetrics: const OutdoorMetrics(),
    );
  }

  static const Object _sentinel = Object();

  final SessionLogStep step;
  final Map<String, Map<String, bool>> checklist;
  final ArcMetrics arcMetrics;
  final HangboardMetrics hangboardMetrics;
  final LimitBoulderMetrics limitBoulderMetrics;
  final CampusMetrics campusMetrics;
  final OutdoorMetrics outdoorMetrics;
  final int rpe;
  final int durationMin;
  final String notes;
  final bool isSaving;
  final bool isSaved;
  final String? errorMessage;

  int get completedChecklistItems => checklist.values.fold(
        0,
        (total, items) =>
            total + items.values.where((isChecked) => isChecked).length,
      );

  int get totalChecklistItems =>
      checklist.values.fold(0, (total, items) => total + items.length);

  bool get isChecklistComplete {
    if (checklist.isEmpty) {
      return true;
    }

    return checklist.values
        .every((items) => items.values.any((isChecked) => isChecked));
  }

  SessionLogState copyWith({
    SessionLogStep? step,
    Map<String, Map<String, bool>>? checklist,
    ArcMetrics? arcMetrics,
    HangboardMetrics? hangboardMetrics,
    LimitBoulderMetrics? limitBoulderMetrics,
    CampusMetrics? campusMetrics,
    OutdoorMetrics? outdoorMetrics,
    int? rpe,
    int? durationMin,
    String? notes,
    bool? isSaving,
    bool? isSaved,
    Object? errorMessage = _sentinel,
  }) {
    return SessionLogState(
      step: step ?? this.step,
      checklist: checklist ?? this.checklist,
      arcMetrics: arcMetrics ?? this.arcMetrics,
      hangboardMetrics: hangboardMetrics ?? this.hangboardMetrics,
      limitBoulderMetrics: limitBoulderMetrics ?? this.limitBoulderMetrics,
      campusMetrics: campusMetrics ?? this.campusMetrics,
      outdoorMetrics: outdoorMetrics ?? this.outdoorMetrics,
      rpe: rpe ?? this.rpe,
      durationMin: durationMin ?? this.durationMin,
      notes: notes ?? this.notes,
      isSaving: isSaving ?? this.isSaving,
      isSaved: isSaved ?? this.isSaved,
      errorMessage: identical(errorMessage, _sentinel)
          ? this.errorMessage
          : errorMessage as String?,
    );
  }

  @override
  List<Object?> get props => [
        step,
        checklist,
        arcMetrics,
        hangboardMetrics,
        limitBoulderMetrics,
        campusMetrics,
        outdoorMetrics,
        rpe,
        durationMin,
        notes,
        isSaving,
        isSaved,
        errorMessage,
      ];
}

class SessionLogNotifier extends StateNotifier<SessionLogState> {
  SessionLogNotifier(this.ref, this.session)
      : super(SessionLogState.initial(session.sessionType));

  final Ref ref;
  final DaySessionDetails session;

  void toggleCheck(String blockId, String item) {
    final block = Map<String, bool>.from(state.checklist[blockId] ?? const {});
    final currentValue = block[item] ?? false;
    block[item] = !currentValue;

    state = state.copyWith(
      checklist: <String, Map<String, bool>>{
        ...state.checklist,
        blockId: block,
      },
      errorMessage: null,
    );
  }

  void nextStep() {
    final nextStep = switch (state.step) {
      SessionLogStep.checklist => SessionLogStep.metrics,
      SessionLogStep.metrics => SessionLogStep.wrapup,
      SessionLogStep.wrapup => SessionLogStep.wrapup,
    };

    state = state.copyWith(step: nextStep, errorMessage: null);
  }

  void prevStep() {
    final previousStep = switch (state.step) {
      SessionLogStep.checklist => SessionLogStep.checklist,
      SessionLogStep.metrics => SessionLogStep.checklist,
      SessionLogStep.wrapup => SessionLogStep.metrics,
    };

    state = state.copyWith(step: previousStep, errorMessage: null);
  }

  void updateRpe(int rpe) =>
      state = state.copyWith(rpe: rpe, errorMessage: null);

  void updateDuration(int minutes) {
    state = state.copyWith(durationMin: minutes, errorMessage: null);
  }

  void updateNotes(String notes) =>
      state = state.copyWith(notes: notes, errorMessage: null);

  void updateArc(ArcMetrics metrics) {
    state = state.copyWith(arcMetrics: metrics, errorMessage: null);
  }

  void updateHangboard(HangboardMetrics metrics) {
    state = state.copyWith(hangboardMetrics: metrics, errorMessage: null);
  }

  void updateLimitBoulder(LimitBoulderMetrics metrics) {
    state = state.copyWith(limitBoulderMetrics: metrics, errorMessage: null);
  }

  void updateCampus(CampusMetrics metrics) {
    state = state.copyWith(campusMetrics: metrics, errorMessage: null);
  }

  void updateOutdoor(OutdoorMetrics metrics) {
    state = state.copyWith(outdoorMetrics: metrics, errorMessage: null);
  }

  Future<void> save(int sessionLogId) async {
    state = state.copyWith(isSaving: true, errorMessage: null);

    final result = await ref.read(sessionLogRepositoryProvider).completeSession(
          sessionLogId: sessionLogId,
          rpe: state.rpe,
          durationMin: state.durationMin,
          notes: state.notes,
          metrics: _buildMetrics(),
        );

    state = result.fold(
      (failure) => state.copyWith(
        isSaving: false,
        isSaved: false,
        errorMessage: failure.message,
      ),
      (_) {
        ref.invalidate(todaySessionProvider);
        ref.invalidate(currentWeekProvider);
        ref.invalidate(statsSummaryProvider);
        return state.copyWith(
          isSaving: false,
          isSaved: true,
          errorMessage: null,
        );
      },
    );
  }

  List<MetricInput> _buildMetrics() {
    return switch (session.sessionType.metricType) {
      'arc' => state.arcMetrics.toMetricInputs(),
      'hangboard' => state.hangboardMetrics.toMetricInputs(),
      'limitBoulder' => state.limitBoulderMetrics.toMetricInputs(),
      'campus' => state.campusMetrics.toMetricInputs(),
      'outdoor' => state.outdoorMetrics.toMetricInputs(),
      _ => const [],
    };
  }
}

final sessionLogProvider = StateNotifierProvider.autoDispose
    .family<SessionLogNotifier, SessionLogState, DaySessionDetails>(
  (ref, session) {
    return SessionLogNotifier(ref, session);
  },
);
