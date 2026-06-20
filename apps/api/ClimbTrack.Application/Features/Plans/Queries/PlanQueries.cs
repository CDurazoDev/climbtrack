using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.Plans.Commands.CreateUserPlan;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using Dapper;
using MediatR;

namespace ClimbTrack.Application.Features.Plans.Queries;

public record GetActivePlanQuery : IRequest<Result<UserPlanDetailDto>>;

public record GetUserPlansQuery : IRequest<List<UserPlanDto>>;

public record GetPlanWeeksQuery(long PlanId) : IRequest<Result<List<PlanWeekDto>>>;

public class GetUserPlansHandler : IRequestHandler<GetUserPlansQuery, List<UserPlanDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetUserPlansHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<List<UserPlanDto>> Handle(GetUserPlansQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        const string sql = """
            select up.id,
                   up.name,
                   tt.code as TrainingTypeCode,
                   dl.code as DifficultyLevelCode,
                   up.start_date as StartDate,
                   up.end_date as EndDate,
                   up.is_active as IsActive,
                   coalesce(avg(upw.progress_pct), 0) as ProgressPct
            from user_plans up
            inner join training_types tt on tt.id = up.training_type_id
            inner join difficulty_levels dl on dl.id = up.difficulty_level_id
            left join user_plan_weeks upw on upw.user_plan_id = up.id
            where up.user_id = @UserId
            group by up.id, up.name, tt.code, dl.code, up.start_date, up.end_date, up.is_active
            order by up.is_active desc, up.start_date desc, up.id desc;
            """;

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<PlanQueryHelper.UserPlanListRow>(sql, new { UserId = _currentUser.UserId.Value });

        return rows
            .Select(row => new UserPlanDto(
                row.Id,
                row.Name,
                row.TrainingTypeCode,
                row.DifficultyLevelCode,
                row.StartDate,
                row.EndDate,
                row.IsActive,
                row.ProgressPct))
            .ToList();
    }
}

public class GetActivePlanHandler : IRequestHandler<GetActivePlanQuery, Result<UserPlanDetailDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetActivePlanHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<Result<UserPlanDetailDto>> Handle(GetActivePlanQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<UserPlanDetailDto>("Authentication is required.");
        }

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var plan = await PlanQueryHelper.GetActivePlanRowAsync(connection, _currentUser.UserId.Value);
        if (plan is null)
        {
            return Result.Failure<UserPlanDetailDto>("No active plan found.");
        }

        var weeks = await PlanQueryHelper.GetPlanWeeksAsync(connection, _currentUser.UserId.Value, plan.Id, ct);
        return Result.Success(new UserPlanDetailDto(
            plan.Id,
            plan.Name,
            plan.TrainingTypeCode,
            plan.DifficultyLevelCode,
            plan.StartDate,
            plan.EndDate,
            plan.IsActive,
            weeks));
    }
}

public class GetPlanWeeksHandler : IRequestHandler<GetPlanWeeksQuery, Result<List<PlanWeekDto>>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetPlanWeeksHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<Result<List<PlanWeekDto>>> Handle(GetPlanWeeksQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<List<PlanWeekDto>>("Authentication is required.");
        }

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var plan = await PlanQueryHelper.GetPlanByIdAsync(connection, _currentUser.UserId.Value, request.PlanId);
        if (plan is null)
        {
            return Result.Failure<List<PlanWeekDto>>("Plan not found.");
        }

        var weeks = await PlanQueryHelper.GetPlanWeeksAsync(connection, _currentUser.UserId.Value, request.PlanId, ct);
        return Result.Success(weeks);
    }
}

internal static class PlanQueryHelper
{
    private static readonly string[] DayLabels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public static async Task<PlanHeaderRow?> GetActivePlanRowAsync(
        System.Data.Common.DbConnection connection,
        long userId)
    {
        const string sql = """
            select up.id,
                   up.name,
                   tt.code as TrainingTypeCode,
                   dl.code as DifficultyLevelCode,
                   up.start_date as StartDate,
                   up.end_date as EndDate,
                   up.is_active as IsActive
            from user_plans up
            inner join training_types tt on tt.id = up.training_type_id
            inner join difficulty_levels dl on dl.id = up.difficulty_level_id
            where up.user_id = @UserId
              and up.is_active = true
            order by up.start_date desc, up.id desc
            limit 1;
            """;

        return await connection.QuerySingleOrDefaultAsync<PlanHeaderRow>(sql, new { UserId = userId });
    }

    public static async Task<PlanHeaderRow?> GetPlanByIdAsync(
        System.Data.Common.DbConnection connection,
        long userId,
        long planId)
    {
        const string sql = """
            select up.id,
                   up.name,
                   tt.code as TrainingTypeCode,
                   dl.code as DifficultyLevelCode,
                   up.start_date as StartDate,
                   up.end_date as EndDate,
                   up.is_active as IsActive
            from user_plans up
            inner join training_types tt on tt.id = up.training_type_id
            inner join difficulty_levels dl on dl.id = up.difficulty_level_id
            where up.user_id = @UserId
              and up.id = @PlanId
            limit 1;
            """;

        return await connection.QuerySingleOrDefaultAsync<PlanHeaderRow>(sql, new { UserId = userId, PlanId = planId });
    }

    public static async Task<List<PlanWeekDto>> GetPlanWeeksAsync(
        System.Data.Common.DbConnection connection,
        long userId,
        long planId,
        CancellationToken ct)
    {
        const string weeksSql = """
            select upw.id,
                   upw.week_number as WeekNumber,
                   ph.name as PhaseName,
                   ph.description as PhaseDescription,
                   upw.progress_pct as ProgressPct,
                   upw.is_deload as IsDeload,
                   upw.start_date as StartDate,
                   upw.plan_template_id as PlanTemplateId
            from user_plan_weeks upw
            inner join user_plans up on up.id = upw.user_plan_id
            inner join phases ph on ph.id = upw.phase_id
            where up.user_id = @UserId
              and up.id = @PlanId
            order by upw.week_number, upw.id;
            """;

        const string templateDaysSql = """
            select ptd.plan_template_id as PlanTemplateId,
                   ptd.day_of_week as DayOfWeek,
                   ptd.is_rest as IsRest,
                   st.code as SessionTypeCode,
                   st.name as SessionTypeName,
                   st.color_hex as SessionColorHex
            from plan_template_days ptd
            left join session_types st on st.id = ptd.session_type_id
            where ptd.plan_template_id in (
                select distinct upw.plan_template_id
                from user_plan_weeks upw
                where upw.user_plan_id = @PlanId
                  and upw.plan_template_id is not null
            );
            """;

        const string logsSql = """
            select usl.id,
                   usl.user_plan_week_id as UserPlanWeekId,
                   usl.log_date as LogDate,
                   usl.day_of_week as DayOfWeek,
                   usl.is_done as IsDone,
                   st.code as SessionTypeCode,
                   st.name as SessionTypeName,
                   st.color_hex as SessionColorHex
            from user_session_logs usl
            inner join session_types st on st.id = usl.session_type_id
            where usl.user_id = @UserId
              and usl.user_plan_week_id in (
                  select id
                  from user_plan_weeks
                  where user_plan_id = @PlanId
              )
            order by usl.log_date, usl.id;
            """;

        var weekRows = (await connection.QueryAsync<PlanWeekRow>(new CommandDefinition(
            weeksSql,
            new { UserId = userId, PlanId = planId },
            cancellationToken: ct))).ToList();

        if (weekRows.Count == 0)
        {
            return [];
        }

        var templateDays = (await connection.QueryAsync<TemplateDayRow>(new CommandDefinition(
            templateDaysSql,
            new { PlanId = planId },
            cancellationToken: ct)))
            .GroupBy(row => (row.PlanTemplateId, row.DayOfWeek))
            .ToDictionary(group => group.Key, group => group.Last());

        var logs = (await connection.QueryAsync<SessionLogStateRow>(new CommandDefinition(
            logsSql,
            new { UserId = userId, PlanId = planId },
            cancellationToken: ct)))
            .GroupBy(row => (row.UserPlanWeekId ?? 0L, row.LogDate))
            .ToDictionary(group => group.Key, group => group.Last());

        var today = DateOnly.FromDateTime(DateTime.Now);

        return weekRows
            .Select(week => BuildWeekDto(week, templateDays, logs, today))
            .ToList();
    }

    private static PlanWeekDto BuildWeekDto(
        PlanWeekRow week,
        IReadOnlyDictionary<(int? PlanTemplateId, int DayOfWeek), TemplateDayRow> templateDays,
        IReadOnlyDictionary<(long UserPlanWeekId, DateOnly LogDate), SessionLogStateRow> logs,
        DateOnly today)
    {
        var days = new List<DayEntryDto>(capacity: 7);
        for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            var date = DateOnly.FromDateTime(week.StartDate.Date.AddDays(dayOfWeek));
            templateDays.TryGetValue((week.PlanTemplateId, dayOfWeek), out var templateDay);
            logs.TryGetValue((week.Id, date), out var log);

            var isRest = templateDay?.IsRest ?? log is null;
            var sessionTypeId = log?.SessionTypeCode ?? templateDay?.SessionTypeCode;
            var sessionTypeName = log?.SessionTypeName ?? templateDay?.SessionTypeName;
            var sessionColorHex = log?.SessionColorHex ?? templateDay?.SessionColorHex;

            var state = isRest
                ? "rest"
                : log?.IsDone == true
                    ? "completed"
                    : date == today
                        ? "today"
                        : date < today
                            ? "failed"
                            : "pending";

            days.Add(new DayEntryDto(
                DayLabels[dayOfWeek],
                state,
                sessionTypeId,
                sessionTypeName,
                sessionColorHex));
        }

        return new PlanWeekDto(
            week.Id,
            week.WeekNumber,
            week.PhaseName,
            PlanPhaseHelper.ExtractColorHex(week.PhaseDescription),
            week.ProgressPct,
            week.IsDeload,
            days);
    }

    internal sealed class PlanHeaderRow
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string TrainingTypeCode { get; init; } = string.Empty;
        public string DifficultyLevelCode { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public bool IsActive { get; init; }
    }

    internal sealed class UserPlanListRow
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string TrainingTypeCode { get; init; } = string.Empty;
        public string DifficultyLevelCode { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public bool IsActive { get; init; }
        public double ProgressPct { get; init; }
    }

    private sealed class PlanWeekRow
    {
        public long Id { get; init; }
        public int WeekNumber { get; init; }
        public string PhaseName { get; init; } = string.Empty;
        public string? PhaseDescription { get; init; }
        public double ProgressPct { get; init; }
        public bool IsDeload { get; init; }
        public DateTime StartDate { get; init; }
        public int? PlanTemplateId { get; init; }
    }

    private sealed class TemplateDayRow
    {
        public int? PlanTemplateId { get; init; }
        public int DayOfWeek { get; init; }
        public bool IsRest { get; init; }
        public string? SessionTypeCode { get; init; }
        public string? SessionTypeName { get; init; }
        public string? SessionColorHex { get; init; }
    }

    private sealed class SessionLogStateRow
    {
        public long Id { get; init; }
        public long? UserPlanWeekId { get; init; }
        public DateOnly LogDate { get; init; }
        public int DayOfWeek { get; init; }
        public bool IsDone { get; init; }
        public string SessionTypeCode { get; init; } = string.Empty;
        public string SessionTypeName { get; init; } = string.Empty;
        public string SessionColorHex { get; init; } = string.Empty;
    }
}
