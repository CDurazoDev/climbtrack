using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.SessionLogs.Queries;

public sealed record GetTodaySessionLogQuery : IRequest<Result<TodaySessionDto?>>;

public sealed class GetTodaySessionLogQueryHandler : IRequestHandler<GetTodaySessionLogQuery, Result<TodaySessionDto?>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTodaySessionLogQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<TodaySessionDto?>> Handle(
        GetTodaySessionLogQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<TodaySessionDto?>("Authentication is required.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessionLog = await _db.UserSessionLogs
            .AsNoTracking()
            .Include(log => log.SessionType)
            .Where(log => log.UserId == _currentUser.UserId.Value && log.LogDate == today)
            .OrderByDescending(log => log.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionLog is null)
        {
            return Result.Success<TodaySessionDto?>(null);
        }

        return Result.Success<TodaySessionDto?>(new TodaySessionDto(
            sessionLog.Id,
            sessionLog.LogDate.ToDateTime(TimeOnly.MinValue),
            sessionLog.SessionTypeId.ToString(),
            sessionLog.SessionType.Name,
            sessionLog.SessionType.ColorHex,
            sessionLog.IsDone,
            sessionLog.Rpe,
            sessionLog.DurationMin,
            sessionLog.Notes));
    }
}
