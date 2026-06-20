using ClimbTrack.Application.Features.Catalogs.Dtos;

namespace ClimbTrack.Application.Features.SessionLogs.Dtos;

public record SessionLogDto(
    long Id,
    DateOnly LogDate,
    string SessionTypeId,
    string SessionTypeName,
    string SessionColorHex,
    bool IsDone,
    int? Rpe,
    int? DurationMin,
    string? Notes);

public record SessionLogDetailDto(
    long Id,
    DateOnly LogDate,
    SessionTypeDto SessionType,
    bool IsDone,
    int? Rpe,
    int? DurationMin,
    string? Notes,
    List<SessionMetricDto> Metrics);

public record SessionMetricDto(string MetricKey, string MetricValue, string? MetricUnit);
