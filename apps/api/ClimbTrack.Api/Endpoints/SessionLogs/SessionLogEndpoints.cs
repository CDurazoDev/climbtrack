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
