using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.SessionLogs.Commands.UpdateSessionLog;

public sealed class UpdateSessionLogCommandHandler : IRequestHandler<UpdateSessionLogCommand, Result<SessionLogDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateSessionLogCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<SessionLogDto>> Handle(UpdateSessionLogCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<SessionLogDto>("Authentication is required.");
        }

        var sessionLog = await _db.UserSessionLogs
            .Include(log => log.SessionType)
            .Include(log => log.Metrics)
            .FirstOrDefaultAsync(
                log => log.Id == request.SessionLogId && log.UserId == _currentUser.UserId.Value,
                cancellationToken);

        if (sessionLog is null)
        {
            return Result.Failure<SessionLogDto>("Session log not found.");
        }

        var nextRpe = sessionLog.IsDone && !request.Rpe.HasValue
            ? sessionLog.Rpe
            : request.Rpe;
        var nextDurationMin = sessionLog.IsDone && !request.DurationMin.HasValue
            ? sessionLog.DurationMin
            : request.DurationMin;

        if (sessionLog.IsDone && (!nextRpe.HasValue || !nextDurationMin.HasValue))
        {
            return Result.Failure<SessionLogDto>(
                "Completed session logs must preserve both RPE and duration values.");
        }

        sessionLog.UpdateDraft(nextRpe, nextDurationMin, request.Notes);
        SessionLogWriteHelper.SyncMetrics(_db, sessionLog, request.Metrics);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(SessionLogWriteHelper.ToDto(sessionLog));
    }
}
