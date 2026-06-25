import 'day_entry_model.dart';

class PlanWeekModel {
  const PlanWeekModel({
    required this.id,
    required this.weekNumber,
    required this.phaseName,
    required this.phaseColorHex,
    required this.progressPct,
    required this.isDeload,
    required this.days,
  });

  factory PlanWeekModel.fromJson(Map<String, dynamic> json) {
    return PlanWeekModel(
      id: json['id'] as int? ?? 0,
      weekNumber: json['weekNumber'] as int? ?? 0,
      phaseName: json['phaseName'] as String? ?? '',
      phaseColorHex: json['phaseColorHex'] as String? ?? '#A0A0A0',
      progressPct: (json['progressPct'] as num?)?.toDouble() ?? 0,
      isDeload: json['isDeload'] as bool? ?? false,
      days: (json['days'] as List<dynamic>? ?? const <dynamic>[])
          .whereType<Map<String, dynamic>>()
          .map(DayEntryModel.fromJson)
          .toList(growable: false),
    );
  }

  final int id;
  final int weekNumber;
  final String phaseName;
  final String phaseColorHex;
  final double progressPct;
  final bool isDeload;
  final List<DayEntryModel> days;

  double get normalizedProgress {
    if (progressPct <= 0) {
      return 0;
    }

    if (progressPct <= 1) {
      return progressPct;
    }

    return (progressPct / 100).clamp(0, 1);
  }

  int get progressPercent => (normalizedProgress * 100).round();
}
