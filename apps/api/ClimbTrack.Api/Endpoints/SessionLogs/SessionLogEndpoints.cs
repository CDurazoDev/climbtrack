using ClimbTrack.Application.Features.SessionLogs.Commands.CompleteSessionLog;
using ClimbTrack.Application.Features.SessionLogs.Commands.CreateSessionLog;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Application.Features.SessionLogs.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.SessionLogs;

public static class SessionLogEndpoints
{
    public static RouteGroupBuilder MapSessionLogs(this RouteGroupBuilder group)
    {
        group.MapGet("/today", GetTodaySession).WithName("GetTodaySession").RequireAuthorization();
        group.MapGet("/", GetSessionLogs).WithName("GetSessionLogs").RequireAuthorization();
        group.MapPost("/", CreateSessionLog)
            .WithName("CreateSessionLog")
            .RequireAuthorization()
            .Produces<SessionLogDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapPost("/{id:long}/complete", CompleteSession)
            .WithName("CompleteSession")
            .RequireAuthorization()
            .Produces<SessionLogDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
        return group;
    }

    private static async Task<IResult> GetTodaySession(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetTodaySessionQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints").LogError(ex, "Failed to get today's session.");
            return Results.Problem("An unexpected server error occurred while loading today's session.");
        }
    }

    private static async Task<IResult> GetSessionLogs(
        long planWeekId,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetWeekSessionLogsQuery(planWeekId), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints").LogError(ex, "Failed to get session logs for week {PlanWeekId}.", planWeekId);
            return Results.Problem("An unexpected server error occurred while loading session logs.");
        }
    }

    private static async Task<IResult> CreateSessionLog(
        CreateSessionLogCommand command,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/session-logs/{result.Value.Id}", result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints").LogError(ex, "Failed to create session log.");
            return Results.Problem("An unexpected server error occurred while creating the session log.");
        }
    }

    private static async Task<IResult> CompleteSession(
        long id,
        CompleteSessionLogCommand command,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(command with { SessionLogId = id }, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints").LogError(ex, "Failed to complete session log {SessionLogId}.", id);
            return Results.Problem("An unexpected server error occurred while completing the session.");
        }
    }
}
