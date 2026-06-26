using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CompleteSessionLog;

public sealed record CompleteSessionLogCommand(
    long SessionLogId,
    int Rpe,
    int DurationMin,
    string? Notes,
    IReadOnlyCollection<MetricInput> Metrics) : IRequest<Result<SessionLogDto>>;
