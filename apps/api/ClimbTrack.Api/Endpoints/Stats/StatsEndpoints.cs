using ClimbTrack.Application.Features.Stats.Queries;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Stats;

public static class StatsEndpoints
{
    public static RouteGroupBuilder MapStats(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", GetSummary)
            .WithName("Stats_GetSummary")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetSummary(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetStatsSummaryQuery(), cancellationToken);
            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("StatsEndpoints")
                .LogError(ex, "Failed to load dashboard stats summary.");
            return Results.Problem("An unexpected server error occurred while loading the dashboard stats summary.");
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
