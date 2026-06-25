namespace ClimbTrack.Application.Features.SessionLogs.Dtos;

public sealed record TodaySessionDto(
    long Id,
    DateTime LogDate,
    string SessionTypeId,
    string SessionTypeName,
    string SessionColorHex,
    bool IsDone,
    int? Rpe,
    int? DurationMin,
    string? Notes);
