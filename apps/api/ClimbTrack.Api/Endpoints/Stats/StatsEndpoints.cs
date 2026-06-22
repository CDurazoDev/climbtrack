using ClimbTrack.Application.Features.Stats.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Stats;

public static class StatsEndpoints
{
    public static RouteGroupBuilder MapStats(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", GetSummary).WithName("Stats_Summary").RequireAuthorization();
        group.MapGet("/weekly-load", GetWeeklyLoad).WithName("Stats_WeeklyLoad").RequireAuthorization();
        group.MapGet("/rpe-history", GetRpeHistory).WithName("Stats_RpeHistory").RequireAuthorization();
        group.MapGet("/energy-distribution", GetEnergyDistribution).WithName("Stats_EnergyDistribution").RequireAuthorization();
        group.MapGet("/session-history", GetSessionHistory).WithName("Stats_SessionHistory").RequireAuthorization();
        return group;
    }

    private static async Task<IResult> GetSummary(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return await Execute(
            mediator,
            loggerFactory,
            cancellationToken,
            new GetStatsSummaryQuery(),
            "Failed to load stats summary.",
            "An unexpected server error occurred while loading stats summary.");
    }

    private static async Task<IResult> GetWeeklyLoad(
        string? range,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return await Execute(
            mediator,
            loggerFactory,
            cancellationToken,
            new GetWeeklyLoadQuery(range ?? "month"),
            "Failed to load weekly load.",
            "An unexpected server error occurred while loading weekly load.");
    }

    private static async Task<IResult> GetRpeHistory(
        string? range,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return await Execute(
            mediator,
            loggerFactory,
            cancellationToken,
            new GetRpeHistoryQuery(range ?? "month"),
            "Failed to load RPE history.",
            "An unexpected server error occurred while loading RPE history.");
    }

    private static async Task<IResult> GetEnergyDistribution(
        string? range,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return await Execute(
            mediator,
            loggerFactory,
            cancellationToken,
            new GetEnergyDistributionQuery(range ?? "month"),
            "Failed to load energy distribution.",
            "An unexpected server error occurred while loading energy distribution.");
    }

    private static async Task<IResult> GetSessionHistory(
        int? limit,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        return await Execute(
            mediator,
            loggerFactory,
            cancellationToken,
            new GetSessionHistoryQuery(limit ?? 20),
            "Failed to load session history.",
            "An unexpected server error occurred while loading session history.");
    }

    private static async Task<IResult> Execute<TResponse>(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken,
        IRequest<TResponse> request,
        string logMessage,
        string problemMessage)
    {
        try
        {
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("StatsEndpoints").LogError(ex, logMessage);
            return Results.Problem(problemMessage);
        }
    }
}
