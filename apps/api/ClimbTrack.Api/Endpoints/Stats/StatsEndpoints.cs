using ClimbTrack.Application.Features.Stats.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Stats;

public static class StatsEndpoints
{
    public static RouteGroupBuilder MapStats(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", GetSummary).WithName("GetStatsSummary").RequireAuthorization();
        group.MapGet("/weekly-load", GetWeeklyLoad).WithName("GetWeeklyLoad").RequireAuthorization();
        group.MapGet("/rpe-history", GetRpeHistory).WithName("GetRpeHistory").RequireAuthorization();
        group.MapGet("/energy-distribution", GetEnergyDistribution).WithName("GetEnergyDistribution").RequireAuthorization();
        return group;
    }

    private static async Task<IResult> GetSummary(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetStatsSummaryQuery(), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("StatsEndpoints").LogError(ex, "Failed to get stats summary.");
            return Results.Problem("An unexpected server error occurred while loading the stats summary.");
        }
    }

    private static async Task<IResult> GetWeeklyLoad(
        string? range,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetWeeklyLoadQuery(range ?? "8w"), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("StatsEndpoints").LogError(ex, "Failed to get weekly load for range {Range}.", range);
            return Results.Problem("An unexpected server error occurred while loading weekly load stats.");
        }
    }

    private static async Task<IResult> GetRpeHistory(
        string? range,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetRpeHistoryQuery(range ?? "8w"), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("StatsEndpoints").LogError(ex, "Failed to get RPE history for range {Range}.", range);
            return Results.Problem("An unexpected server error occurred while loading RPE history.");
        }
    }

    private static async Task<IResult> GetEnergyDistribution(
        string? range,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetEnergyDistributionQuery(range ?? "8w"), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("StatsEndpoints").LogError(ex, "Failed to get energy distribution for range {Range}.", range);
            return Results.Problem("An unexpected server error occurred while loading energy distribution.");
        }
    }
}
