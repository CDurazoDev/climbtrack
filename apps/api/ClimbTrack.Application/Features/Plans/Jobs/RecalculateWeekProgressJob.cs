using ClimbTrack.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Plans.Jobs;

public class RecalculateWeekProgressJob
{
    private readonly IApplicationDbContext _db;

    public RecalculateWeekProgressJob(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var weeks = await _db.UserPlanWeeks
            .AsNoTracking()
            .Select(week => new
            {
                week.Id,
                PlannedSessions = _db.UserSessionLogs.Count(log => log.UserPlanWeekId == week.Id),
                CompletedSessions = _db.UserSessionLogs.Count(log => log.UserPlanWeekId == week.Id && log.IsDone)
            })
            .ToListAsync(cancellationToken);

        foreach (var week in weeks)
        {
            var progress = week.PlannedSessions == 0
                ? 0
                : Math.Round((double)week.CompletedSessions / week.PlannedSessions * 100, 2);

            await _db.UserPlanWeeks
                .Where(entity => entity.Id == week.Id)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(entity => entity.ProgressPct, progress),
                    cancellationToken);
        }
    }
}
