using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Stats.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Entities;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Stats.Queries;

public sealed record GetStatsSummaryQuery : IRequest<Result<StatsSummaryDto>>;

public sealed class GetStatsSummaryQueryHandler : IRequestHandler<GetStatsSummaryQuery, Result<StatsSummaryDto>>
{
    private sealed record CompletedLogData(DateOnly LogDate, int? Rpe, long? UserPlanWeekId, int DayOfWeek);
    private sealed record EndedWeekData(long Id, double ProgressPct, int? PlanTemplateId);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetStatsSummaryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<StatsSummaryDto>> Handle(
        GetStatsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<StatsSummaryDto>("Authentication is required.");
        }

        var userId = _currentUser.UserId.Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = StartOfWeek(today);
        var nextWeekStart = weekStart.AddDays(7);

        var completedLogs = await _db.UserSessionLogs
            .AsNoTracking()
            .Where(log => log.UserId == userId && log.IsDone)
            .Select(log => new CompletedLogData(log.LogDate, log.Rpe, log.UserPlanWeekId, log.DayOfWeek))
            .ToListAsync(cancellationToken);

        var totalSessions = completedLogs.Count;
        var sessionsThisWeek = completedLogs.Count(log => log.LogDate >= weekStart && log.LogDate < nextWeekStart);
        var rpeValues = completedLogs
            .Where(log => log.Rpe.HasValue)
            .Select(log => log.Rpe!.Value)
            .ToList();
        var rpeAverage = rpeValues.Count == 0
            ? 0
            : Math.Round(rpeValues.Average(), 1, MidpointRounding.AwayFromZero);

        var completedDates = completedLogs
            .Select(log => log.LogDate)
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        var currentStreak = CalculateCurrentStreak(completedDates, today);
        var maxStreak = CalculateMaxStreak(completedDates);
        var weeksCompleted = await CalculateWeeksCompletedAsync(userId, today, completedLogs, cancellationToken);

        return Result.Success(new StatsSummaryDto(
            totalSessions,
            currentStreak,
            maxStreak,
            rpeAverage,
            sessionsThisWeek,
            weeksCompleted));
    }

    private async Task<int> CalculateWeeksCompletedAsync(
        long userId,
        DateOnly today,
        List<CompletedLogData> completedLogs,
        CancellationToken cancellationToken)
    {
        var completedLogsByWeek = completedLogs
            .Where(log => log.UserPlanWeekId.HasValue)
            .GroupBy(log => log.UserPlanWeekId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.Select(log => log.DayOfWeek).Distinct().Count());

        var todayDate = today.ToDateTime(TimeOnly.MinValue);

        var endedWeeks = await _db.UserPlanWeeks
            .AsNoTracking()
            .Where(week =>
                week.Plan.UserId == userId &&
                week.StartDate.AddDays(7) <= todayDate)
            .Select(week => new EndedWeekData(week.Id, week.ProgressPct, week.PlanTemplateId))
            .ToListAsync(cancellationToken);

        if (endedWeeks.Count == 0)
        {
            return 0;
        }

        var templateIds = endedWeeks
            .Where(week => week.PlanTemplateId.HasValue)
            .Select(week => week.PlanTemplateId!.Value)
            .Distinct()
            .ToList();

        var scheduledDaysByTemplate = templateIds.Count == 0
            ? new Dictionary<int, int>()
            : await _db.PlanTemplateDays
                .AsNoTracking()
                .Where(day => templateIds.Contains(day.PlanTemplateId) && !day.IsRest)
                .GroupBy(day => day.PlanTemplateId)
                .Select(group => new
                {
                    PlanTemplateId = group.Key,
                    ScheduledDays = group.Count()
                })
                .ToDictionaryAsync(item => item.PlanTemplateId, item => item.ScheduledDays, cancellationToken);

        return endedWeeks.Count(week =>
        {
            if (week.PlanTemplateId.HasValue &&
                scheduledDaysByTemplate.TryGetValue(week.PlanTemplateId.Value, out var scheduledDays) &&
                scheduledDays > 0)
            {
                return completedLogsByWeek.TryGetValue(week.Id, out var completedDays) &&
                       completedDays >= scheduledDays;
            }

            return week.ProgressPct >= 100;
        });
    }

    private static int CalculateCurrentStreak(IReadOnlyList<DateOnly> completedDates, DateOnly today)
    {
        if (completedDates.Count == 0 || completedDates[^1] < today.AddDays(-1))
        {
            return 0;
        }

        var streak = 1;
        for (var index = completedDates.Count - 1; index > 0; index--)
        {
            if (completedDates[index].DayNumber - completedDates[index - 1].DayNumber != 1)
            {
                break;
            }

            streak++;
        }

        return streak;
    }

    private static int CalculateMaxStreak(IReadOnlyList<DateOnly> completedDates)
    {
        if (completedDates.Count == 0)
        {
            return 0;
        }

        var maxStreak = 1;
        var currentStreak = 1;

        for (var index = 1; index < completedDates.Count; index++)
        {
            if (completedDates[index].DayNumber - completedDates[index - 1].DayNumber == 1)
            {
                currentStreak++;
                maxStreak = Math.Max(maxStreak, currentStreak);
                continue;
            }

            currentStreak = 1;
        }

        return maxStreak;
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var dayOfWeek = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-dayOfWeek);
    }
}
