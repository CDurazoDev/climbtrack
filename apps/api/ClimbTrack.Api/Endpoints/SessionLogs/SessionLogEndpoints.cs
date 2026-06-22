using ClimbTrack.Application.Features.Stats.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.SessionLogs;

public static class SessionLogEndpoints
{
    public static RouteGroupBuilder MapSessionLogs(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetSessionHistory)
            .WithName("SessionLogs_History")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetSessionHistory(
        int? limit,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new GetSessionHistoryQuery(limit ?? 20),
                cancellationToken);

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("SessionLogEndpoints")
                .LogError(ex, "Failed to load session history.");
            return Results.Problem("An unexpected server error occurred while loading session history.");
        }
    }
}
