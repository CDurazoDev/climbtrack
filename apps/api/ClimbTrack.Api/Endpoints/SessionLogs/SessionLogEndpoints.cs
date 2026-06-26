using ClimbTrack.Application.Features.SessionLogs.Commands.CompleteSessionLog;
using ClimbTrack.Application.Features.SessionLogs.Commands.CreateSessionLog;
using ClimbTrack.Application.Features.SessionLogs.Commands.UpdateSessionLog;
using ClimbTrack.Application.Features.SessionLogs.Dtos;
using ClimbTrack.Application.Features.SessionLogs.Queries;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Api.Endpoints.SessionLogs;

public static class SessionLogEndpoints
{
    public static RouteGroupBuilder MapSessionLogs(this RouteGroupBuilder group)
    {
        group.MapGet("/today", GetTodaySessionLog)
            .WithName("SessionLogs_GetToday")
            .RequireAuthorization();

        group.MapPost("/", CreateSessionLog)
            .WithName("SessionLogs_Create")
            .RequireAuthorization()
            .Produces<SessionLogDto>(StatusCodes.Status201Created);

        group.MapPut("/{id:long}", UpdateSessionLog)
            .WithName("SessionLogs_Update")
            .RequireAuthorization();

        group.MapPost("/{id:long}/complete", CompleteSessionLog)
            .WithName("SessionLogs_Complete")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetTodaySessionLog(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetTodaySessionLogQuery(), cancellationToken);
            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints")
                .LogError(ex, "Failed to load today's session log.");
            return Results.Problem("An unexpected server error occurred while loading today's session log.");
        }
    }

    private static async Task<IResult> CreateSessionLog(
        CreateSessionLogRequest request,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new CreateSessionLogCommand(
                    request.UserPlanWeekId,
                    request.SessionTypeId,
                    request.LogDate,
                    request.DayOfWeek),
                cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Created($"/session-logs/{result.Value.Id}", result.Value);
            }

            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints")
                .LogError(ex, "Failed to create session log.");
            return Results.Problem("An unexpected server error occurred while creating the session log.");
        }
    }

    private static async Task<IResult> UpdateSessionLog(
        long id,
        UpdateSessionLogRequest request,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new UpdateSessionLogCommand(
                    id,
                    request.Rpe,
                    request.DurationMin,
                    request.Notes,
                    (request.Metrics ?? [])
                    .Select(metric => new MetricInput(metric.Key, metric.Value, metric.Unit))
                    .ToArray()),
                cancellationToken);

            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints")
                .LogError(ex, "Failed to update session log {SessionLogId}.", id);
            return Results.Problem("An unexpected server error occurred while updating the session log.");
        }
    }

    private static async Task<IResult> CompleteSessionLog(
        long id,
        CompleteSessionLogRequest request,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new CompleteSessionLogCommand(
                    id,
                    request.Rpe,
                    request.DurationMin,
                    request.Notes,
                    (request.Metrics ?? [])
                    .Select(metric => new MetricInput(metric.Key, metric.Value, metric.Unit))
                    .ToArray()),
                cancellationToken);

            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints")
                .LogError(ex, "Failed to complete session log {SessionLogId}.", id);
            return Results.Problem("An unexpected server error occurred while completing the session log.");
        }
    }

    private static IResult ToHttpResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error ?? "The request could not be completed.";
        var statusCode = error.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return Results.Problem(error, statusCode: statusCode);
    }
}
