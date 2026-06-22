class SessionLogDto {
  const SessionLogDto({
    required this.id,
    required this.logDate,
    required this.sessionTypeId,
    required this.sessionTypeName,
    required this.sessionColorHex,
    required this.isDone,
    required this.rpe,
    required this.durationMin,
    required this.notes,
  });

  factory SessionLogDto.fromJson(Map<String, dynamic> json) {
    return SessionLogDto(
      id: (json['id'] as num).toInt(),
      logDate: DateTime.parse(json['logDate'] as String),
      sessionTypeId: json['sessionTypeId'] as String? ?? '',
      sessionTypeName: json['sessionTypeName'] as String? ?? '',
      sessionColorHex: json['sessionColorHex'] as String? ?? '#5C5C5C',
      isDone: json['isDone'] as bool? ?? false,
      rpe: (json['rpe'] as num?)?.toInt(),
      durationMin: (json['durationMin'] as num?)?.toInt(),
      notes: json['notes'] as String?,
    );
  }

  final int id;
  final DateTime logDate;
  final String sessionTypeId;
  final String sessionTypeName;
  final String sessionColorHex;
  final bool isDone;
  final int? rpe;
  final int? durationMin;
  final String? notes;
}

class StatsSummaryDto {
  const StatsSummaryDto({
    required this.totalSessions,
    required this.currentStreak,
    required this.maxStreak,
    required this.rpeAverage,
    required this.sessionsThisWeek,
    required this.weeksCompleted,
  });

  factory StatsSummaryDto.fromJson(Map<String, dynamic> json) {
    return StatsSummaryDto(
      totalSessions: (json['totalSessions'] as num?)?.toInt() ?? 0,
      currentStreak: (json['currentStreak'] as num?)?.toInt() ?? 0,
      maxStreak: (json['maxStreak'] as num?)?.toInt() ?? 0,
      rpeAverage: (json['rpeAverage'] as num?)?.toDouble() ?? 0,
      sessionsThisWeek: (json['sessionsThisWeek'] as num?)?.toInt() ?? 0,
      weeksCompleted: (json['weeksCompleted'] as num?)?.toInt() ?? 0,
    );
  }

  final int totalSessions;
  final int currentStreak;
  final int maxStreak;
  final double rpeAverage;
  final int sessionsThisWeek;
  final int weeksCompleted;
}
