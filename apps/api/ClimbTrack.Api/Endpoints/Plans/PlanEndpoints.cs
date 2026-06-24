using ClimbTrack.Application.Features.Plans.Queries;
using ClimbTrack.Domain.Common;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Plans;

public static class PlanEndpoints
{
    public static RouteGroupBuilder MapPlans(this RouteGroupBuilder group)
    {
        group.MapGet("/active", GetActivePlan)
            .WithName("Plans_GetActivePlan")
            .RequireAuthorization();

        group.MapGet("/active/weeks/current", GetCurrentWeek)
            .WithName("Plans_GetCurrentWeek")
            .RequireAuthorization();

        group.MapGet("/{planId:long}/weeks/{weekNumber:int}", GetPlanWeekDetail)
            .WithName("Plans_GetPlanWeekDetail")
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> GetActivePlan(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetActivePlanQuery(), cancellationToken);
            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints")
                .LogError(ex, "Failed to load active plan.");
            return Results.Problem("An unexpected server error occurred while loading the active plan.");
        }
    }

    private static async Task<IResult> GetCurrentWeek(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetCurrentPlanWeekQuery(), cancellationToken);
            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints")
                .LogError(ex, "Failed to load current plan week.");
            return Results.Problem("An unexpected server error occurred while loading the current plan week.");
        }
    }

    private static async Task<IResult> GetPlanWeekDetail(
        long planId,
        int weekNumber,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetPlanWeekDetailQuery(planId, weekNumber), cancellationToken);
            return ToHttpResult(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints")
                .LogError(ex, "Failed to load plan week detail for plan {PlanId} week {WeekNumber}.", planId, weekNumber);
            return Results.Problem("An unexpected server error occurred while loading the plan week detail.");
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
