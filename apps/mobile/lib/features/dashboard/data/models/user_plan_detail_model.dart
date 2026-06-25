import 'plan_week_model.dart';

class UserPlanDetailModel {
  const UserPlanDetailModel({
    required this.id,
    required this.name,
    required this.trainingTypeCode,
    required this.difficultyLevelCode,
    required this.startDate,
    this.endDate,
    required this.isActive,
    required this.weeks,
  });

  factory UserPlanDetailModel.fromJson(Map<String, dynamic> json) {
    return UserPlanDetailModel(
      id: json['id'] as int? ?? 0,
      name: json['name'] as String? ?? '',
      trainingTypeCode: json['trainingTypeCode'] as String? ?? '',
      difficultyLevelCode: json['difficultyLevelCode'] as String? ?? '',
      startDate: DateTime.tryParse(json['startDate'] as String? ?? '') ?? DateTime.now(),
      endDate: json['endDate'] == null
          ? null
          : DateTime.tryParse(json['endDate'] as String) ?? DateTime.now(),
      isActive: json['isActive'] as bool? ?? false,
      weeks: (json['weeks'] as List<dynamic>? ?? const <dynamic>[])
          .whereType<Map<String, dynamic>>()
          .map(PlanWeekModel.fromJson)
          .toList(growable: false),
    );
  }

  final int id;
  final String name;
  final String trainingTypeCode;
  final String difficultyLevelCode;
  final DateTime startDate;
  final DateTime? endDate;
  final bool isActive;
  final List<PlanWeekModel> weeks;
}
