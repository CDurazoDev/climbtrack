using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.CreateSessionLog;

public record CreateSessionLogCommand(
    long? UserPlanWeekId,
    string SessionTypeId,
    DateOnly LogDate,
    int DayOfWeek) : IRequest<Result<SessionLogDto>>;

public class CreateSessionLogCommandValidator : AbstractValidator<CreateSessionLogCommand>
{
    public CreateSessionLogCommandValidator()
    {
        RuleFor(command => command.SessionTypeId).NotEmpty().MaximumLength(50);
        RuleFor(command => command.DayOfWeek).InclusiveBetween(0, 6);
        RuleFor(command => command.LogDate).NotEmpty();
    }
}

public class CreateSessionLogCommandHandler : IRequestHandler<CreateSessionLogCommand, Result<SessionLogDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateSessionLogCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<SessionLogDto>> Handle(CreateSessionLogCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<SessionLogDto>("Authentication is required.");
        }

        var expectedDayOfWeek = ((int)request.LogDate.DayOfWeek + 6) % 7;
        if (request.DayOfWeek != expectedDayOfWeek)
        {
            return Result.Failure<SessionLogDto>("DayOfWeek must match the provided LogDate using Monday=0 through Sunday=6.");
        }

        var sessionType = await _db.SessionTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Code == request.SessionTypeId, ct);

        if (sessionType is null)
        {
            return Result.Failure<SessionLogDto>("Session type not found.");
        }

        if (request.UserPlanWeekId.HasValue)
        {
            var weekExists = await _db.UserPlanWeeks
                .AnyAsync(
                    week => week.Id == request.UserPlanWeekId.Value &&
                            week.Plan.UserId == _currentUser.UserId.Value,
                    ct);

            if (!weekExists)
            {
                return Result.Failure<SessionLogDto>("The selected plan week does not belong to the current user.");
            }
        }

        var log = new UserSessionLog(
            _currentUser.UserId.Value,
            sessionType.Id,
            request.LogDate,
            request.DayOfWeek,
            false,
            null,
            null,
            null,
            request.UserPlanWeekId);

        _db.UserSessionLogs.Add(log);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new SessionLogDto(
            log.Id,
            log.LogDate,
            sessionType.Code,
            sessionType.Name,
            sessionType.ColorHex,
            log.IsDone,
            log.Rpe,
            log.DurationMin,
            log.Notes));
    }
}
