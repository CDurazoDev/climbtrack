namespace ClimbTrack.Application.Features.SessionLogs.Dtos;

public sealed record MetricInput(
    string Key,
    string Value,
    string? Unit);
