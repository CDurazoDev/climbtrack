import 'package:equatable/equatable.dart';

class MetricInput extends Equatable {
  const MetricInput({
    required this.key,
    required this.value,
    this.unit,
  });

  final String key;
  final String value;
  final String? unit;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'key': key,
        'value': value,
        if (unit != null && unit!.isNotEmpty) 'unit': unit,
      };

  @override
  List<Object?> get props => [key, value, unit];
}

class ArcMetrics extends Equatable {
  const ArcMetrics({
    this.series = 2,
    this.durationMin = 20,
    this.pumpLevel = 3,
  });

  final int series;
  final int durationMin;
  final int pumpLevel;

  ArcMetrics copyWith({
    int? series,
    int? durationMin,
    int? pumpLevel,
  }) {
    return ArcMetrics(
      series: series ?? this.series,
      durationMin: durationMin ?? this.durationMin,
      pumpLevel: pumpLevel ?? this.pumpLevel,
    );
  }

  List<MetricInput> toMetricInputs() => [
        MetricInput(key: 'series_count', value: series.toString()),
        MetricInput(
          key: 'series_duration_min',
          value: durationMin.toString(),
          unit: 'min',
        ),
        MetricInput(key: 'pump_level', value: pumpLevel.toString()),
      ];

  @override
  List<Object?> get props => [series, durationMin, pumpLevel];
}

class HangboardMetrics extends Equatable {
  const HangboardMetrics({
    this.gripType = 'Punta 4 dedos',
    this.addedWeightKg = 0,
    this.sets = 4,
    this.reps = 6,
    this.hangDurationS = 5,
  });

  final String gripType;
  final double addedWeightKg;
  final int sets;
  final int reps;
  final int hangDurationS;

  HangboardMetrics copyWith({
    String? gripType,
    double? addedWeightKg,
    int? sets,
    int? reps,
    int? hangDurationS,
  }) {
    return HangboardMetrics(
      gripType: gripType ?? this.gripType,
      addedWeightKg: addedWeightKg ?? this.addedWeightKg,
      sets: sets ?? this.sets,
      reps: reps ?? this.reps,
      hangDurationS: hangDurationS ?? this.hangDurationS,
    );
  }

  List<MetricInput> toMetricInputs() => [
        MetricInput(key: 'grip_type', value: gripType),
        MetricInput(
          key: 'added_weight_kg',
          value: _formatNumber(addedWeightKg),
          unit: 'kg',
        ),
        MetricInput(key: 'sets_count', value: sets.toString()),
        MetricInput(key: 'reps_count', value: reps.toString()),
        MetricInput(
          key: 'hang_duration_s',
          value: hangDurationS.toString(),
          unit: 's',
        ),
      ];

  @override
  List<Object?> get props => [gripType, addedWeightKg, sets, reps, hangDurationS];
}

class LimitBoulderMetrics extends Equatable {
  const LimitBoulderMetrics({
    this.targetGrade = '7B',
    this.problemsWorked = 2,
    this.progressPct = 60,
  });

  final String targetGrade;
  final int problemsWorked;
  final int progressPct;

  LimitBoulderMetrics copyWith({
    String? targetGrade,
    int? problemsWorked,
    int? progressPct,
  }) {
    return LimitBoulderMetrics(
      targetGrade: targetGrade ?? this.targetGrade,
      problemsWorked: problemsWorked ?? this.problemsWorked,
      progressPct: progressPct ?? this.progressPct,
    );
  }

  List<MetricInput> toMetricInputs() => [
        MetricInput(key: 'target_grade', value: targetGrade),
        MetricInput(key: 'problems_worked', value: problemsWorked.toString()),
        MetricInput(
          key: 'progress_pct',
          value: progressPct.toString(),
          unit: '%',
        ),
      ];

  @override
  List<Object?> get props => [targetGrade, problemsWorked, progressPct];
}

class CampusMetrics extends Equatable {
  const CampusMetrics({
    this.exercise = '1-3-5',
    this.rungs = '1-3-5',
    this.sets = 4,
  });

  final String exercise;
  final String rungs;
  final int sets;

  CampusMetrics copyWith({
    String? exercise,
    String? rungs,
    int? sets,
  }) {
    return CampusMetrics(
      exercise: exercise ?? this.exercise,
      rungs: rungs ?? this.rungs,
      sets: sets ?? this.sets,
    );
  }

  List<MetricInput> toMetricInputs() => [
        MetricInput(key: 'exercise', value: exercise),
        MetricInput(key: 'rungs', value: rungs),
        MetricInput(key: 'sets_count', value: sets.toString()),
      ];

  @override
  List<Object?> get props => [exercise, rungs, sets];
}

class OutdoorMetrics extends Equatable {
  const OutdoorMetrics({
    this.sector = '',
    this.redpoints = 0,
    this.projectAttempts = 0,
    this.conditions = 'Buenas',
  });

  final String sector;
  final int redpoints;
  final int projectAttempts;
  final String conditions;

  OutdoorMetrics copyWith({
    String? sector,
    int? redpoints,
    int? projectAttempts,
    String? conditions,
  }) {
    return OutdoorMetrics(
      sector: sector ?? this.sector,
      redpoints: redpoints ?? this.redpoints,
      projectAttempts: projectAttempts ?? this.projectAttempts,
      conditions: conditions ?? this.conditions,
    );
  }

  List<MetricInput> toMetricInputs() => [
        MetricInput(key: 'sector', value: sector),
        MetricInput(key: 'redpoints', value: redpoints.toString()),
        MetricInput(
          key: 'project_attempts',
          value: projectAttempts.toString(),
        ),
        MetricInput(key: 'conditions', value: conditions),
      ];

  @override
  List<Object?> get props => [sector, redpoints, projectAttempts, conditions];
}

String _formatNumber(num value) {
  if (value == value.roundToDouble()) {
    return value.toStringAsFixed(0);
  }

  return value.toStringAsFixed(1);
}
