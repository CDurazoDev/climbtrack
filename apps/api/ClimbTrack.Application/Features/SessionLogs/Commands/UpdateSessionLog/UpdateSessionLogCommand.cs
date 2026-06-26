using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.UpdateSessionLog;

public sealed record UpdateSessionLogCommand(
    long SessionLogId,
    int? Rpe,
    int? DurationMin,
    string? Notes,
    IReadOnlyCollection<MetricInput> Metrics) : IRequest<Result<SessionLogDto>>;
