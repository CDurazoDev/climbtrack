namespace ClimbTrack.Application.Features.SessionLogs.Dtos;

public sealed record SessionLogDto(
    long Id,
    DateOnly LogDate,
    string SessionTypeId,
    string SessionTypeName,
    string SessionColorHex,
    bool IsDone,
    int? Rpe,
    int? DurationMin,
    string? Notes);
