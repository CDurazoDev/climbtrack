class StatsSummaryModel {
  const StatsSummaryModel({
    required this.totalSessions,
    required this.currentStreak,
    required this.maxStreak,
    required this.rpeAverage,
    required this.sessionsThisWeek,
    required this.weeksCompleted,
  });

  factory StatsSummaryModel.fromJson(Map<String, dynamic> json) {
    return StatsSummaryModel(
      totalSessions: json['totalSessions'] as int? ?? 0,
      currentStreak: json['currentStreak'] as int? ?? 0,
      maxStreak: json['maxStreak'] as int? ?? 0,
      rpeAverage: (json['rpeAverage'] as num?)?.toDouble() ?? 0,
      sessionsThisWeek: json['sessionsThisWeek'] as int? ?? 0,
      weeksCompleted: json['weeksCompleted'] as int? ?? 0,
    );
  }

  final int totalSessions;
  final int currentStreak;
  final int maxStreak;
  final double rpeAverage;
  final int sessionsThisWeek;
  final int weeksCompleted;
}
