namespace ClimbTrack.Api.Endpoints.SessionLogs;

public sealed record CreateSessionLogRequest(
    long? UserPlanWeekId,
    string SessionTypeId,
    DateOnly LogDate,
    int DayOfWeek);

public sealed record UpdateSessionLogRequest(
    int? Rpe,
    int? DurationMin,
    string? Notes,
    List<MetricInputRequest>? Metrics);

public sealed record CompleteSessionLogRequest(
    int Rpe,
    int DurationMin,
    string? Notes,
    List<MetricInputRequest>? Metrics);

public sealed record MetricInputRequest(
    string Key,
    string Value,
    string? Unit);
