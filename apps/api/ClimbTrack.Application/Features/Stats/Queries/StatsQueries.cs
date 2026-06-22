using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Stats.Dtos;
using ClimbTrack.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Application.Features.Stats.Queries;

public record GetStatsSummaryQuery : IRequest<StatsSummaryDto>;

public record GetWeeklyLoadQuery(string Range) : IRequest<List<WeeklyLoadDto>>;

public record GetRpeHistoryQuery(string Range) : IRequest<List<RpeDataPointDto>>;

public record GetEnergyDistributionQuery(string Range) : IRequest<EnergyDistributionDto>;

public record GetSessionHistoryQuery(int Limit) : IRequest<List<SessionHistoryDto>>;

public class GetStatsSummaryQueryHandler : IRequestHandler<GetStatsSummaryQuery, StatsSummaryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetStatsSummaryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<StatsSummaryDto> Handle(GetStatsSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return new StatsSummaryDto(0, 0, 0, 0, 0, 0);
        }

        var userId = _currentUser.UserId.Value;
        var completedLogs = await _db.UserSessionLogs
            .AsNoTracking()
            .Where(log => log.UserId == userId && log.IsDone)
            .OrderBy(log => log.LogDate)
            .ToListAsync(cancellationToken);

        var totalSessions = completedLogs.Count;
        var rpeAverage = completedLogs.Count == 0
            ? 0
            : completedLogs.Where(log => log.Rpe.HasValue).DefaultIfEmpty()
                .Average(log => (double?)log?.Rpe ?? 0);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfWeek = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var sessionsThisWeek = completedLogs.Count(log => log.LogDate >= startOfWeek);

        var currentStreak = CalculateCurrentStreak(completedLogs.Select(log => log.LogDate).Distinct().ToList(), today);
        var maxStreak = CalculateMaxStreak(completedLogs.Select(log => log.LogDate).Distinct().ToList());

        var completedWeeks = await _db.UserPlanWeeks
            .AsNoTracking()
            .CountAsync(
                week => week.Plan.UserId == userId && week.ProgressPct >= 100,
                cancellationToken);

        return new StatsSummaryDto(
            totalSessions,
            currentStreak,
            maxStreak,
            Math.Round(rpeAverage, 1),
            sessionsThisWeek,
            completedWeeks);
    }

    private static int CalculateCurrentStreak(List<DateOnly> dates, DateOnly today)
    {
        if (dates.Count == 0)
        {
            return 0;
        }

        var ordered = dates.OrderByDescending(date => date).ToList();
        var cursor = ordered[0] == today ? today : today.AddDays(-1);
        var streak = 0;

        foreach (var date in ordered)
        {
            if (date == cursor)
            {
                streak++;
                cursor = cursor.AddDays(-1);
                continue;
            }

            if (date < cursor)
            {
                break;
            }
        }

        return streak;
    }

    private static int CalculateMaxStreak(List<DateOnly> dates)
    {
        if (dates.Count == 0)
        {
            return 0;
        }

        var ordered = dates.OrderBy(date => date).ToList();
        var best = 1;
        var current = 1;

        for (var index = 1; index < ordered.Count; index++)
        {
            if (ordered[index] == ordered[index - 1].AddDays(1))
            {
                current++;
                best = Math.Max(best, current);
            }
            else
            {
                current = 1;
            }
        }

        return best;
    }
}

public class GetWeeklyLoadQueryHandler : IRequestHandler<GetWeeklyLoadQuery, List<WeeklyLoadDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetWeeklyLoadQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<WeeklyLoadDto>> Handle(GetWeeklyLoadQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        var rangeStart = StatsRangeHelper.ResolveRangeStart(request.Range);
        var logs = await _db.UserSessionLogs
            .AsNoTracking()
            .Include(log => log.SessionType)
            .Where(log => log.UserId == _currentUser.UserId.Value && log.IsDone && log.LogDate >= rangeStart)
            .OrderBy(log => log.LogDate)
            .ToListAsync(cancellationToken);

        return logs
            .GroupBy(log => log.LogDate.AddDays(-log.DayOfWeek))
            .Select(group => new WeeklyLoadDto(
                group.Key.ToString("MM/dd"),
                group.Count(log => log.SessionType.Code == "arc"),
                group.Count(log => log.SessionType.Code == "hangboard"),
                group.Count(log => log.SessionType.Code.StartsWith("campus")),
                group.Count(log =>
                    log.SessionType.Code is "limit" or "boulder" or "linked"),
                group.Count(log => log.SessionType.Code == "outdoor")))
            .ToList();
    }
}

public class GetRpeHistoryQueryHandler : IRequestHandler<GetRpeHistoryQuery, List<RpeDataPointDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetRpeHistoryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<RpeDataPointDto>> Handle(GetRpeHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        var rangeStart = StatsRangeHelper.ResolveRangeStart(request.Range);
        var logs = await _db.UserSessionLogs
            .AsNoTracking()
            .Where(log =>
                log.UserId == _currentUser.UserId.Value &&
                log.IsDone &&
                log.Rpe.HasValue &&
                log.LogDate >= rangeStart)
            .OrderBy(log => log.LogDate)
            .ToListAsync(cancellationToken);

        var history = new List<RpeDataPointDto>();
        double runningSum = 0;

        for (var index = 0; index < logs.Count; index++)
        {
            runningSum += logs[index].Rpe!.Value;
            history.Add(new RpeDataPointDto(
                logs[index].LogDate.ToString("MM/dd"),
                logs[index].Rpe!.Value,
                Math.Round(runningSum / (index + 1), 1)));
        }

        return history;
    }
}

public class GetEnergyDistributionQueryHandler : IRequestHandler<GetEnergyDistributionQuery, EnergyDistributionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetEnergyDistributionQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EnergyDistributionDto> Handle(GetEnergyDistributionQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return new EnergyDistributionDto(0, 0, 0);
        }

        var rangeStart = StatsRangeHelper.ResolveRangeStart(request.Range);
        var logs = await _db.UserSessionLogs
            .AsNoTracking()
            .Include(log => log.SessionType)
            .ThenInclude(sessionType => sessionType.EnergySystem)
            .Where(log => log.UserId == _currentUser.UserId.Value && log.IsDone && log.LogDate >= rangeStart)
            .ToListAsync(cancellationToken);

        var total = logs.Count;
        if (total == 0)
        {
            return new EnergyDistributionDto(0, 0, 0);
        }

        double Percentage(string code) =>
            Math.Round(logs.Count(log => log.SessionType.EnergySystem.Code == code) * 100d / total, 1);

        return new EnergyDistributionDto(
            Percentage("alactico"),
            Percentage("aerobico"),
            Percentage("lactico"));
    }
}

public class GetSessionHistoryQueryHandler : IRequestHandler<GetSessionHistoryQuery, List<SessionHistoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetSessionHistoryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<SessionHistoryDto>> Handle(GetSessionHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        var limit = request.Limit <= 0 ? 20 : Math.Min(request.Limit, 100);
        var logs = await _db.UserSessionLogs
            .AsNoTracking()
            .Include(log => log.SessionType)
            .Where(log => log.UserId == _currentUser.UserId.Value && log.IsDone)
            .OrderByDescending(log => log.LogDate)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return logs
            .Select(log => new SessionHistoryDto(
                log.Id,
                log.LogDate,
                log.SessionType.Code,
                log.SessionType.Name,
                log.SessionType.ColorHex,
                log.Rpe,
                log.DurationMin))
            .ToList();
    }
}

internal static class StatsRangeHelper
{
    public static DateOnly ResolveRangeStart(string range)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return range.ToLowerInvariant() switch
        {
            "week" => today.AddDays(-6),
            "month" => today.AddMonths(-1),
            "3m" or "3months" or "quarter" => today.AddMonths(-3),
            _ => DateOnly.MinValue
        };
    }
}
