using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CreateSessionLog;

public sealed record CreateSessionLogCommand(
    long? UserPlanWeekId,
    string SessionTypeId,
    DateOnly LogDate,
    int DayOfWeek) : IRequest<Result<SessionLogDto>>;
