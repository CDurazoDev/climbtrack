using ClimbTrack.Application.Common.Interfaces;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Domain.Common;
using ClimbTrack.Domain.Interfaces;
using Dapper;
using MediatR;

namespace ClimbTrack.Application.Features.SessionLogs.Queries;

public record GetTodaySessionQuery : IRequest<Result<SessionLogDto>>;

public record GetWeekSessionLogsQuery(long PlanWeekId) : IRequest<List<SessionLogDto>>;

public class GetTodaySessionHandler : IRequestHandler<GetTodaySessionQuery, Result<SessionLogDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetTodaySessionHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<Result<SessionLogDto>> Handle(GetTodaySessionQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return Result.Failure<SessionLogDto>("Authentication is required.");
        }

        const string sql = """
            select usl.id,
                   usl.log_date as LogDate,
                   st.code as SessionTypeId,
                   st.name as SessionTypeName,
                   st.color_hex as SessionColorHex,
                   usl.is_done as IsDone,
                   usl.rpe,
                   usl.duration_min as DurationMin,
                   usl.notes
            from user_session_logs usl
            inner join session_types st on st.id = usl.session_type_id
            where usl.user_id = @UserId
              and usl.log_date = current_date
            order by usl.id desc
            limit 1;
            """;

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var log = await connection.QuerySingleOrDefaultAsync<SessionLogDto>(sql, new { UserId = _currentUser.UserId.Value });

        return log is null
            ? Result.Failure<SessionLogDto>("No session found for today.")
            : Result.Success(log);
    }
}

public class GetWeekSessionLogsHandler : IRequestHandler<GetWeekSessionLogsQuery, List<SessionLogDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUser;

    public GetWeekSessionLogsHandler(ISqlConnectionFactory sqlConnectionFactory, ICurrentUserService currentUser)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUser = currentUser;
    }

    public async Task<List<SessionLogDto>> Handle(GetWeekSessionLogsQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
        {
            return [];
        }

        const string sql = """
            select usl.id,
                   usl.log_date as LogDate,
                   st.code as SessionTypeId,
                   st.name as SessionTypeName,
                   st.color_hex as SessionColorHex,
                   usl.is_done as IsDone,
                   usl.rpe,
                   usl.duration_min as DurationMin,
                   usl.notes
            from user_session_logs usl
            inner join session_types st on st.id = usl.session_type_id
            inner join user_plan_weeks upw on upw.id = usl.user_plan_week_id
            inner join user_plans up on up.id = upw.user_plan_id
            where usl.user_id = @UserId
              and usl.user_plan_week_id = @PlanWeekId
              and up.user_id = @UserId
            order by usl.log_date, usl.id;
            """;

        await using var connection = await _sqlConnectionFactory.OpenConnectionAsync(ct);
        var logs = await connection.QueryAsync<SessionLogDto>(sql, new
        {
            UserId = _currentUser.UserId.Value,
            request.PlanWeekId
        });

        return logs.ToList();
    }
}
