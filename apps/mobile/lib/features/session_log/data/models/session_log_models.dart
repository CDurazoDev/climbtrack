import 'package:equatable/equatable.dart';

import '../../../dashboard/data/models/dashboard_models.dart';
import '../../../plan/data/models/plan_models.dart';
import '../../domain/entities/session_metrics.dart';

class SessionBlockDto extends Equatable {
  const SessionBlockDto({
    required this.id,
    required this.name,
    required this.sortOrder,
    required this.items,
  });

  factory SessionBlockDto.fromJson(Map<String, dynamic> json) {
    return SessionBlockDto(
      id: (json['id'] as num?)?.toInt() ?? 0,
      name: json['name'] as String? ?? '',
      sortOrder: (json['sortOrder'] as num?)?.toInt() ?? 0,
      items: (json['items'] as List<dynamic>? ?? const [])
          .map((item) => item as String)
          .toList(),
    );
  }

  final int id;
  final String name;
  final int sortOrder;
  final List<String> items;

  @override
  List<Object?> get props => [id, name, sortOrder, items];
}

class SessionTypeDto extends Equatable {
  const SessionTypeDto({
    required this.id,
    required this.code,
    required this.name,
    required this.colorHex,
    required this.loadLevel,
    required this.description,
    required this.energySystemCode,
    required this.energySystemName,
    required this.blocks,
  });

  factory SessionTypeDto.fromJson(Map<String, dynamic> json) {
    return SessionTypeDto(
      id: (json['id'] as num?)?.toInt() ?? 0,
      code: json['code'] as String? ?? '',
      name: json['name'] as String? ?? '',
      colorHex: json['colorHex'] as String? ?? '#5C5C5C',
      loadLevel: (json['loadLevel'] as num?)?.toInt() ?? 0,
      description: json['description'] as String?,
      energySystemCode: json['energySystemCode'] as String? ?? '',
      energySystemName: json['energySystemName'] as String? ?? '',
      blocks: (json['blocks'] as List<dynamic>? ?? const [])
          .map((item) => SessionBlockDto.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }

  final int id;
  final String code;
  final String name;
  final String colorHex;
  final int loadLevel;
  final String? description;
  final String energySystemCode;
  final String energySystemName;
  final List<SessionBlockDto> blocks;

  String get metricType {
    if (code == 'arc') {
      return 'arc';
    }
    if (code == 'hangboard') {
      return 'hangboard';
    }
    if (code.startsWith('campus')) {
      return 'campus';
    }
    if (code == 'limit' || code == 'boulder' || code == 'linked') {
      return 'limitBoulder';
    }
    if (code == 'outdoor') {
      return 'outdoor';
    }
    if (code == 'rest') {
      return 'rest';
    }
    return 'unknown';
  }

  bool get isRest => code == 'rest';

  @override
  List<Object?> get props => [
        id,
        code,
        name,
        colorHex,
        loadLevel,
        description,
        energySystemCode,
        energySystemName,
        blocks,
      ];
}

class CreateSessionLogRequest extends Equatable {
  const CreateSessionLogRequest({
    required this.userPlanWeekId,
    required this.sessionTypeId,
    required this.logDate,
    required this.dayOfWeek,
  });

  final int? userPlanWeekId;
  final String sessionTypeId;
  final DateTime logDate;
  final int dayOfWeek;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'userPlanWeekId': userPlanWeekId,
        'sessionTypeId': sessionTypeId,
        'logDate': _dateOnly(logDate),
        'dayOfWeek': dayOfWeek,
      };

  @override
  List<Object?> get props => [userPlanWeekId, sessionTypeId, logDate, dayOfWeek];
}

class CompleteSessionRequest extends Equatable {
  const CompleteSessionRequest({
    required this.rpe,
    required this.durationMin,
    this.notes,
    required this.metrics,
  });

  final int rpe;
  final int durationMin;
  final String? notes;
  final List<MetricInput> metrics;

  Map<String, dynamic> toJson() => <String, dynamic>{
        'rpe': rpe,
        'durationMin': durationMin,
        'notes': notes,
        'metrics': metrics.map((metric) => metric.toJson()).toList(),
      };

  @override
  List<Object?> get props => [rpe, durationMin, notes, metrics];
}

class DaySessionDetails extends Equatable {
  const DaySessionDetails({
    required this.weekId,
    required this.dayOfWeek,
    required this.logDate,
    required this.dayEntry,
    required this.sessionType,
    required this.sessionLog,
  });

  final int weekId;
  final int dayOfWeek;
  final DateTime logDate;
  final DayEntryDto dayEntry;
  final SessionTypeDto sessionType;
  final SessionLogDto sessionLog;

  @override
  List<Object?> get props => [
        weekId,
        dayOfWeek,
        logDate,
        dayEntry,
        sessionType,
        sessionLog,
      ];
}

String _dateOnly(DateTime value) {
  final year = value.year.toString().padLeft(4, '0');
  final month = value.month.toString().padLeft(2, '0');
  final day = value.day.toString().padLeft(2, '0');
  return '$year-$month-$day';
}
