using ClimbTrack.Application.Features.Plans.Commands.CreateUserPlan;
using ClimbTrack.Application.Features.Plans.Dtos;
using ClimbTrack.Application.Features.Plans.Queries;
using MediatR;

namespace ClimbTrack.Api.Endpoints.Plans;

public static class PlanEndpoints
{
    public static RouteGroupBuilder MapPlans(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetUserPlans).WithName("GetUserPlans").RequireAuthorization();
        group.MapGet("/active", GetActivePlan).WithName("GetActivePlan").RequireAuthorization();
        group.MapGet("/active/weeks/current", GetCurrentActiveWeek)
            .WithName("GetCurrentActiveWeek")
            .RequireAuthorization();
        group.MapPost("/", CreatePlan)
            .WithName("CreatePlan")
            .RequireAuthorization()
            .Produces<UserPlanDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapGet("/{id:long}/weeks", GetPlanWeeks).WithName("GetPlanWeeks").RequireAuthorization();
        group.MapGet("/{id:long}/weeks/{weekNumber:int}", GetPlanWeekDetail)
            .WithName("GetPlanWeekDetail")
            .RequireAuthorization();
        return group;
    }

    private static async Task<IResult> GetUserPlans(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetUserPlansQuery(), ct);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get user plans.");
            return Results.Problem("An unexpected server error occurred while loading plans.");
        }
    }

    private static async Task<IResult> GetActivePlan(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetActivePlanQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get active plan.");
            return Results.Problem("An unexpected server error occurred while loading the active plan.");
        }
    }

    private static async Task<IResult> CreatePlan(
        CreateUserPlanCommand command,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/plans/{result.Value.Id}", result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to create plan.");
            return Results.Problem("An unexpected server error occurred while creating the plan.");
        }
    }

    private static async Task<IResult> GetPlanWeeks(
        long id,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetPlanWeeksQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get plan weeks for {PlanId}.", id);
            return Results.Problem("An unexpected server error occurred while loading the plan weeks.");
        }
    }

    private static async Task<IResult> GetCurrentActiveWeek(
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetActivePlanQuery(), ct);
            if (!result.IsSuccess)
            {
                return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
            }

            var plan = result.Value;
            if (plan.Weeks.Count == 0)
            {
                return Results.Problem("The active plan does not contain weeks.", statusCode: StatusCodes.Status404NotFound);
            }

            var today = DateTime.Today;
            var elapsedDays = Math.Max(0, (today - plan.StartDate.Date).Days);
            var currentWeekNumber = Math.Clamp((elapsedDays / 7) + 1, 1, plan.Weeks.Max(week => week.WeekNumber));
            var currentWeek = plan.Weeks.FirstOrDefault(week => week.WeekNumber == currentWeekNumber)
                              ?? plan.Weeks.Last();

            return Results.Ok(currentWeek);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(ex, "Failed to get current active week.");
            return Results.Problem("An unexpected server error occurred while loading the current week.");
        }
    }

    private static async Task<IResult> GetPlanWeekDetail(
        long id,
        int weekNumber,
        ISender mediator,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetPlanWeeksQuery(id), ct);
            if (!result.IsSuccess)
            {
                return Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound);
            }

            var week = result.Value.FirstOrDefault(item => item.WeekNumber == weekNumber);
            return week is not null
                ? Results.Ok(week)
                : Results.Problem("Plan week not found.", statusCode: StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            loggerFactory.CreateLogger("PlanEndpoints").LogError(
                ex,
                "Failed to get plan week detail for {PlanId} week {WeekNumber}.",
                id,
                weekNumber);
            return Results.Problem("An unexpected server error occurred while loading the plan week detail.");
        }
    }
}
