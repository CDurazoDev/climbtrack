using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Stats.Dtos;
using ClimbTrack.Domain.Interfaces;
using Dapper;
using MediatR;

namespace ClimbTrack.Application.Features.Stats.Queries;

public record GetStatsSummaryQuery : IRequest<StatsSummaryDto>;

public record GetWeeklyLoadQuery(string Range) : IRequest<List<WeeklyLoadDto>>;

public record GetRpeHistoryQuery(string Range) : IRequest<List<RpeDataPointDto>>;

public record GetEnergyDistributionQuery(string Range) : IRequest<EnergyDistributionDto>;

public class GetStatsSummaryHandler : IRequestHandler<GetStatsSummaryQuery, StatsSummaryDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetStatsSummaryHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<StatsSummaryDto> Handle(GetStatsSummaryQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return new StatsSummaryDto(0, 0, 0, 0, 0, 0);
        }

        const string summarySql = """
            select
                count(*) filter (where is_done = true) as TotalSessions,
                coalesce(avg(rpe) filter (where is_done = true and rpe is not null), 0) as RpeAverage,
                count(*) filter (
                    where is_done = true
                      and log_date between @WeekStart and @WeekEnd
                ) as SessionsThisWeek
            from user_session_logs
            where user_id = @UserId;

            select count(*)
            from user_plan_weeks upw
            inner join user_plans up on up.id = upw.user_plan_id
            where up.user_id = @UserId
              and upw.progress_pct >= 100;

            select distinct log_date as LogDate
            from user_session_logs
            where user_id = @UserId
              and is_done = true
            order by log_date;
            """;

        var today = DateOnly.FromDateTime(DateTime.Now);
        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var weekEnd = weekStart.AddDays(6);

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        using var multi = await connection.QueryMultipleAsync(summarySql, new
        {
            UserId = _currentUser.UserId.Value,
            WeekStart = weekStart,
            WeekEnd = weekEnd
        });

        var summary = await multi.ReadSingleAsync<StatsHelper.SummaryRow>();
        var weeksCompleted = await multi.ReadSingleAsync<int>();
        var sessionDates = (await multi.ReadAsync<StatsHelper.DateRow>()).Select(row => row.LogDate).ToList();
        var streaks = StatsHelper.CalculateStreaks(sessionDates, today);

        return new StatsSummaryDto(
            summary.TotalSessions,
            streaks.CurrentStreak,
            streaks.MaxStreak,
            Math.Round(summary.RpeAverage, 2),
            summary.SessionsThisWeek,
            weeksCompleted);
    }
}

public class GetWeeklyLoadHandler : IRequestHandler<GetWeeklyLoadQuery, List<WeeklyLoadDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetWeeklyLoadHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<List<WeeklyLoadDto>> Handle(GetWeeklyLoadQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        const string sql = """
            select date_trunc('week', usl.log_date::timestamp)::date as WeekStart,
                   st.code as SessionTypeCode,
                   count(*) as Total
            from user_session_logs usl
            inner join session_types st on st.id = usl.session_type_id
            where usl.user_id = @UserId
              and usl.is_done = true
              and usl.log_date >= @FromDate
            group by date_trunc('week', usl.log_date::timestamp)::date, st.code
            order by WeekStart;
            """;

        var weekCount = StatsHelper.ParseWeekRange(request.Range);
        var currentWeekStart = StatsHelper.GetCurrentWeekStart(DateOnly.FromDateTime(DateTime.Now));
        var fromDate = currentWeekStart.AddDays(-(weekCount - 1) * 7);

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<StatsHelper.WeeklyLoadRow>(sql, new
        {
            UserId = _currentUser.UserId.Value,
            FromDate = fromDate
        });

        return StatsHelper.BuildWeeklyLoad(rows, currentWeekStart, weekCount);
    }
}

public class GetRpeHistoryHandler : IRequestHandler<GetRpeHistoryQuery, List<RpeDataPointDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetRpeHistoryHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<List<RpeDataPointDto>> Handle(GetRpeHistoryQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        const string sql = """
            select log_date as LogDate,
                   cast(round(avg(rpe)) as integer) as Rpe
            from user_session_logs
            where user_id = @UserId
              and is_done = true
              and rpe is not null
              and log_date >= @FromDate
            group by log_date
            order by log_date;
            """;

        var weekCount = StatsHelper.ParseWeekRange(request.Range);
        var fromDate = StatsHelper.GetCurrentWeekStart(DateOnly.FromDateTime(DateTime.Now)).AddDays(-(weekCount - 1) * 7);

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var rows = (await connection.QueryAsync<StatsHelper.RpeRow>(sql, new
        {
            UserId = _currentUser.UserId.Value,
            FromDate = fromDate
        })).ToList();

        return StatsHelper.BuildRpeHistory(rows);
    }
}

public class GetEnergyDistributionHandler : IRequestHandler<GetEnergyDistributionQuery, EnergyDistributionDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetEnergyDistributionHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<EnergyDistributionDto> Handle(GetEnergyDistributionQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return new EnergyDistributionDto(0, 0, 0);
        }

        const string sql = """
            select es.code as EnergySystemCode,
                   count(*) as Total
            from user_session_logs usl
            inner join session_types st on st.id = usl.session_type_id
            inner join energy_systems es on es.id = st.energy_system_id
            where usl.user_id = @UserId
              and usl.is_done = true
              and usl.log_date >= @FromDate
            group by es.code;
            """;

        var weekCount = StatsHelper.ParseWeekRange(request.Range);
        var fromDate = StatsHelper.GetCurrentWeekStart(DateOnly.FromDateTime(DateTime.Now)).AddDays(-(weekCount - 1) * 7);

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<StatsHelper.EnergyRow>(sql, new
        {
            UserId = _currentUser.UserId.Value,
            FromDate = fromDate
        });

        return StatsHelper.BuildEnergyDistribution(rows);
    }
}

internal static class StatsHelper
{
    public static int ParseWeekRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return 8;
        }

        var normalized = range.Trim().ToLowerInvariant();
        if (normalized.EndsWith("w") &&
            int.TryParse(normalized[..^1], out var weeks) &&
            weeks > 0)
        {
            return weeks;
        }

        return 8;
    }

    public static DateOnly GetCurrentWeekStart(DateOnly today)
    {
        return today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
    }

    public static (int CurrentStreak, int MaxStreak) CalculateStreaks(IReadOnlyList<DateOnly> sessionDates, DateOnly today)
    {
        if (sessionDates.Count == 0)
        {
            return (0, 0);
        }

        var orderedDates = sessionDates.Distinct().OrderBy(date => date).ToList();
        var maxStreak = 1;
        var streak = 1;
        for (var index = 1; index < orderedDates.Count; index++)
        {
            if (orderedDates[index] == orderedDates[index - 1].AddDays(1))
            {
                streak++;
                maxStreak = Math.Max(maxStreak, streak);
            }
            else
            {
                streak = 1;
            }
        }

        var currentStreak = 0;
        var cursor = orderedDates[^1];
        if (cursor == today || cursor == today.AddDays(-1))
        {
            currentStreak = 1;
            for (var index = orderedDates.Count - 2; index >= 0; index--)
            {
                if (orderedDates[index] == cursor.AddDays(-1))
                {
                    currentStreak++;
                    cursor = orderedDates[index];
                    continue;
                }

                break;
            }
        }

        return (currentStreak, maxStreak);
    }

    public static List<WeeklyLoadDto> BuildWeeklyLoad(
        IEnumerable<WeeklyLoadRow> rows,
        DateOnly currentWeekStart,
        int weekCount)
    {
        var lookup = rows
            .GroupBy(row => DateOnly.FromDateTime(row.WeekStart))
            .ToDictionary(group => group.Key, group => group.ToList());

        var result = new List<WeeklyLoadDto>(capacity: weekCount);
        for (var offset = weekCount - 1; offset >= 0; offset--)
        {
            var weekStart = currentWeekStart.AddDays(-offset * 7);
            lookup.TryGetValue(weekStart, out var weekRows);
            weekRows ??= [];

            result.Add(new WeeklyLoadDto(
                weekStart.ToString("yyyy-MM-dd"),
                CountByCategory(weekRows, "arc"),
                CountByCategory(weekRows, "hangboard"),
                CountByCategory(weekRows, "campus"),
                CountByCategory(weekRows, "boulder"),
                CountByCategory(weekRows, "outdoor"),
                CountByCategory(weekRows, "other")));
        }

        return result;
    }

    public static List<RpeDataPointDto> BuildRpeHistory(IReadOnlyList<RpeRow> rows)
    {
        var result = new List<RpeDataPointDto>(capacity: rows.Count);
        for (var index = 0; index < rows.Count; index++)
        {
            var window = rows.Skip(Math.Max(0, index - 6)).Take(Math.Min(index + 1, 7)).ToList();
            var movingAverage = window.Count == 0 ? 0 : window.Average(item => item.Rpe);
            result.Add(new RpeDataPointDto(
                rows[index].LogDate.ToString("yyyy-MM-dd"),
                rows[index].Rpe,
                Math.Round(movingAverage, 2)));
        }

        return result;
    }

    public static EnergyDistributionDto BuildEnergyDistribution(IEnumerable<EnergyRow> rows)
    {
        var totals = rows.ToDictionary(
            row => row.EnergySystemCode,
            row => row.Total,
            StringComparer.OrdinalIgnoreCase);

        return new EnergyDistributionDto(
            totals.GetValueOrDefault("alactico"),
            totals.GetValueOrDefault("lactico"),
            totals.GetValueOrDefault("aerobico"));
    }

    private static int CountByCategory(IEnumerable<WeeklyLoadRow> rows, string category)
    {
        return category switch
        {
            "arc" => rows.Where(row => row.SessionTypeCode.Equals("arc", StringComparison.OrdinalIgnoreCase)).Sum(row => row.Total),
            "hangboard" => rows.Where(row => row.SessionTypeCode.Equals("hangboard", StringComparison.OrdinalIgnoreCase)).Sum(row => row.Total),
            "campus" => rows.Where(row => row.SessionTypeCode.StartsWith("campus", StringComparison.OrdinalIgnoreCase)).Sum(row => row.Total),
            "boulder" => rows.Where(row =>
                    row.SessionTypeCode.Equals("boulder", StringComparison.OrdinalIgnoreCase) ||
                    row.SessionTypeCode.Equals("limit", StringComparison.OrdinalIgnoreCase) ||
                    row.SessionTypeCode.Equals("linked", StringComparison.OrdinalIgnoreCase))
                .Sum(row => row.Total),
            "outdoor" => rows.Where(row => row.SessionTypeCode.Equals("outdoor", StringComparison.OrdinalIgnoreCase)).Sum(row => row.Total),
            "other" => rows.Where(row =>
                    !row.SessionTypeCode.Equals("arc", StringComparison.OrdinalIgnoreCase) &&
                    !row.SessionTypeCode.Equals("hangboard", StringComparison.OrdinalIgnoreCase) &&
                    !row.SessionTypeCode.StartsWith("campus", StringComparison.OrdinalIgnoreCase) &&
                    !row.SessionTypeCode.Equals("boulder", StringComparison.OrdinalIgnoreCase) &&
                    !row.SessionTypeCode.Equals("limit", StringComparison.OrdinalIgnoreCase) &&
                    !row.SessionTypeCode.Equals("linked", StringComparison.OrdinalIgnoreCase) &&
                    !row.SessionTypeCode.Equals("outdoor", StringComparison.OrdinalIgnoreCase))
                .Sum(row => row.Total),
            _ => 0
        };
    }

    internal sealed class SummaryRow
    {
        public int TotalSessions { get; init; }
        public double RpeAverage { get; init; }
        public int SessionsThisWeek { get; init; }
    }

    internal sealed class DateRow
    {
        public DateOnly LogDate { get; init; }
    }

    internal sealed class WeeklyLoadRow
    {
        public DateTime WeekStart { get; init; }
        public string SessionTypeCode { get; init; } = string.Empty;
        public int Total { get; init; }
    }

    internal sealed class RpeRow
    {
        public DateOnly LogDate { get; init; }
        public int Rpe { get; init; }
    }

    internal sealed class EnergyRow
    {
        public string EnergySystemCode { get; init; } = string.Empty;
        public int Total { get; init; }
    }
}
