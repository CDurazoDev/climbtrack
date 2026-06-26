using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CreateSessionLog;

public sealed class CreateSessionLogCommandHandler : IRequestHandler<CreateSessionLogCommand, Result<SessionLogDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateSessionLogCommandHandler> _logger;

    public CreateSessionLogCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ILogger<CreateSessionLogCommandHandler> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<SessionLogDto>> Handle(CreateSessionLogCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<SessionLogDto>("Authentication is required.");
        }

        if (!int.TryParse(request.SessionTypeId, out var sessionTypeId))
        {
            return Result.Failure<SessionLogDto>("SessionTypeId must be a valid numeric identifier.");
        }

        var sessionType = await _db.SessionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Id == sessionTypeId, cancellationToken);

        if (sessionType is null)
        {
            return Result.Failure<SessionLogDto>("Session type not found.");
        }

        if (request.UserPlanWeekId.HasValue)
        {
            var planWeek = await _db.UserPlanWeeks
                .AsNoTracking()
                .Include(week => week.Plan)
                .FirstOrDefaultAsync(
                    week => week.Id == request.UserPlanWeekId.Value &&
                            week.Plan.UserId == _currentUser.UserId.Value,
                    cancellationToken);

            if (planWeek is null)
            {
                _logger.LogInformation(
                    "Session log creation rejected because plan week {PlanWeekId} was not found for user {UserId}.",
                    request.UserPlanWeekId.Value,
                    _currentUser.UserId.Value);
                return Result.Failure<SessionLogDto>("Plan week not found.");
            }

            if (planWeek.PlanTemplateId.HasValue)
            {
                var templateDay = await _db.PlanTemplateDays
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        day => day.PlanTemplateId == planWeek.PlanTemplateId.Value &&
                               day.DayOfWeek == request.DayOfWeek,
                        cancellationToken);

                if (templateDay is null || templateDay.IsRest || !templateDay.SessionTypeId.HasValue)
                {
                    return Result.Failure<SessionLogDto>("No planned session exists for the provided plan week and day.");
                }

                if (templateDay.SessionTypeId.Value != sessionTypeId)
                {
                    return Result.Failure<SessionLogDto>("The provided session type does not match the planned session.");
                }
            }
        }

        var existingLog = await _db.UserSessionLogs
            .Include(log => log.SessionType)
            .FirstOrDefaultAsync(
                log =>
                    log.UserId == _currentUser.UserId.Value &&
                    log.LogDate == request.LogDate &&
                    log.DayOfWeek == request.DayOfWeek &&
                    log.SessionTypeId == sessionTypeId &&
                    log.UserPlanWeekId == request.UserPlanWeekId,
                cancellationToken);

        if (existingLog is not null)
        {
            return Result.Success(SessionLogWriteHelper.ToDto(existingLog));
        }

        var sessionLog = new UserSessionLog(
            _currentUser.UserId.Value,
            sessionTypeId,
            request.LogDate,
            request.DayOfWeek,
            false,
            null,
            null,
            null,
            request.UserPlanWeekId);

        _db.UserSessionLogs.Add(sessionLog);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new SessionLogDto(
            sessionLog.Id,
            sessionLog.LogDate,
            sessionType.Id.ToString(),
            sessionType.Name,
            sessionType.ColorHex,
            sessionLog.IsDone,
            sessionLog.Rpe,
            sessionLog.DurationMin,
            sessionLog.Notes));
    }
}
