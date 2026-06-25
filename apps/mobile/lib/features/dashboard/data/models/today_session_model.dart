class TodaySessionModel {
  const TodaySessionModel({
    required this.id,
    required this.logDate,
    required this.sessionTypeId,
    required this.sessionTypeName,
    required this.sessionColorHex,
    required this.isDone,
    this.rpe,
    this.durationMin,
    this.notes,
  });

  factory TodaySessionModel.fromJson(Map<String, dynamic> json) {
    return TodaySessionModel(
      id: json['id'] as int? ?? 0,
      logDate: DateTime.tryParse(json['logDate'] as String? ?? '') ?? DateTime.now(),
      sessionTypeId: json['sessionTypeId'] as String? ?? '',
      sessionTypeName: json['sessionTypeName'] as String? ?? '',
      sessionColorHex: json['sessionColorHex'] as String? ?? '#A0A0A0',
      isDone: json['isDone'] as bool? ?? false,
      rpe: json['rpe'] as int?,
      durationMin: json['durationMin'] as int?,
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
