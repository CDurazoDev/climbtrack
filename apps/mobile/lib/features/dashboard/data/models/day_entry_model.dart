class DayEntryModel {
  const DayEntryModel({
    required this.label,
    required this.state,
    this.sessionTypeId,
    this.sessionTypeName,
    this.sessionColorHex,
  });

  factory DayEntryModel.fromJson(Map<String, dynamic> json) {
    return DayEntryModel(
      label: json['label'] as String? ?? '',
      state: json['state'] as String? ?? 'rest',
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
