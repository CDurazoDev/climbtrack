class UserPlanDetailDto {
  const UserPlanDetailDto({
    required this.id,
    required this.name,
    required this.trainingTypeCode,
    required this.difficultyLevelCode,
    required this.startDate,
    required this.endDate,
    required this.isActive,
    required this.weeks,
  });

  factory UserPlanDetailDto.fromJson(Map<String, dynamic> json) {
    return UserPlanDetailDto(
      id: (json['id'] as num).toInt(),
      name: json['name'] as String? ?? '',
      trainingTypeCode: json['trainingTypeCode'] as String? ?? '',
      difficultyLevelCode: json['difficultyLevelCode'] as String? ?? '',
      startDate: DateTime.parse(json['startDate'] as String),
      endDate: json['endDate'] == null
          ? null
          : DateTime.parse(json['endDate'] as String),
      isActive: json['isActive'] as bool? ?? false,
      weeks: (json['weeks'] as List<dynamic>? ?? const [])
          .map((item) => PlanWeekDto.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }

  final int id;
  final String name;
  final String trainingTypeCode;
  final String difficultyLevelCode;
  final DateTime startDate;
  final DateTime? endDate;
  final bool isActive;
  final List<PlanWeekDto> weeks;
}

class PlanWeekDto {
  const PlanWeekDto({
    required this.id,
    required this.weekNumber,
    required this.phaseName,
    required this.phaseColorHex,
    required this.progressPct,
    required this.isDeload,
    required this.days,
  });

  factory PlanWeekDto.fromJson(Map<String, dynamic> json) {
    return PlanWeekDto(
      id: (json['id'] as num).toInt(),
      weekNumber: (json['weekNumber'] as num).toInt(),
      phaseName: json['phaseName'] as String? ?? '',
      phaseColorHex: json['phaseColorHex'] as String? ?? '#5C5C5C',
      progressPct: (json['progressPct'] as num?)?.toDouble() ?? 0,
      isDeload: json['isDeload'] as bool? ?? false,
      days: (json['days'] as List<dynamic>? ?? const [])
          .map((item) => DayEntryDto.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }

  final int id;
  final int weekNumber;
  final String phaseName;
  final String phaseColorHex;
  final double progressPct;
  final bool isDeload;
  final List<DayEntryDto> days;
}

class DayEntryDto {
  const DayEntryDto({
    required this.label,
    required this.state,
    required this.sessionTypeId,
    required this.sessionTypeName,
    required this.sessionColorHex,
  });

  factory DayEntryDto.fromJson(Map<String, dynamic> json) {
    return DayEntryDto(
      label: json['label'] as String? ?? '',
      state: json['state'] as String? ?? 'pending',
      sessionTypeId: json['sessionTypeId'] as String?,
      sessionTypeName: json['sessionTypeName'] as String?,
      sessionColorHex: json['sessionColorHex'] as String?,
    );
  }

  final String label;
  final String state;
  final String? sessionTypeId;
  final String? sessionTypeName;
  final String? sessionColorHex;
}
